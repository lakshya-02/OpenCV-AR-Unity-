using OpenCvSharp.Demo;
using OpenCvSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CounterFinder : WebCamera
{
    [SerializeField] private FlipMode ImageFlip;
    [SerializeField] private float Threshold = 96.4f;
    [SerializeField] private bool ShowProcessingImage = true;
    [SerializeField] private float CurveAccuracy = 10f;
    [SerializeField] private float MinArea = 5000f;
    [SerializeField] private PolygonCollider2D polygonCollider;
    [Header("AR Placement")]
    [SerializeField] private RectTransform uiRoot; // Set to the same RectTransform as WebCamera.Surface (or its parent with matching local space)
    [SerializeField] private RectTransform boardPrefab; // Prefab root with TicTacToeBoard + 3x3 cells
    private RectTransform spawnedBoard;
    private float boardAngleDeg;

    private Mat image;
    private Mat processImage = new Mat();
    private Point[][] contours;
    private HierarchyIndex[] hierarchy;
    private Vector2[] vectorList;

    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {
        image = OpenCvSharp.Unity.TextureToMat(input);

        Cv2.Flip(image, image, ImageFlip);
        Cv2.CvtColor(image, processImage, ColorConversionCodes.BGR2GRAY);
        Cv2.Threshold(processImage, processImage, Threshold, 255, ThresholdTypes.BinaryInv);
        Cv2.FindContours(processImage, out contours, out hierarchy, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple, null);
        polygonCollider.pathCount = 0;

        foreach (Point[] contour in contours)
        {
            Point[] points = Cv2.ApproxPolyDP(contour, CurveAccuracy, true);
            var area = Cv2.ContourArea(points);
            if (area > MinArea)
            {
                drawContour(ShowProcessingImage ? processImage : image, new Scalar(127, 127, 127), 2, points);
                polygonCollider.pathCount++;
                polygonCollider.SetPath(polygonCollider.pathCount - 1, tovector2(points));

                // If we find a 4-corner polygon, treat it as a plane and place board
                if (points.Length == 4)
                {
                    TryPlaceBoard(points, input.width, input.height);
                }
            }
        }

        // Try to detect ball and map to board cell
        if (spawnedBoard != null)
        {
            if (TryDetectBall(processImage, out var center))
            {
                TryMarkByBall(center, input.width, input.height);
            }
        }

        if (output == null)
        {
            output = OpenCvSharp.Unity.MatToTexture(ShowProcessingImage ? processImage : image);
        }
        else
        {
            OpenCvSharp.Unity.MatToTexture(ShowProcessingImage ? processImage : image, output);
        }
        return true;
    }

    private void TryPlaceBoard(Point[] quad, int texW, int texH)
    {
        if (uiRoot == null || boardPrefab == null) return;
        if (spawnedBoard == null)
        {
            spawnedBoard = Instantiate(boardPrefab, uiRoot);
        }

        // Order the quad roughly clockwise starting from top-left in image space
        System.Array.Sort(quad, (a, b) => a.X == b.X ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));
        Point tl = quad[0].Y < quad[1].Y ? quad[0] : quad[1];
        Point bl = quad[0].Y < quad[1].Y ? quad[1] : quad[0];
        Point tr = quad[2].Y < quad[3].Y ? quad[2] : quad[3];
        Point br = quad[2].Y < quad[3].Y ? quad[3] : quad[2];

        // Map to board rect (we use its current size)
        var boardRect = spawnedBoard;
        Vector2 size = boardRect.sizeDelta;
        Point2f[] src = new[] { new Point2f(tl.X, tl.Y), new Point2f(tr.X, tr.Y), new Point2f(br.X, br.Y), new Point2f(bl.X, bl.Y) };
        Point2f[] dst = new[] { new Point2f(0, 0), new Point2f(size.x, 0), new Point2f(size.x, size.y), new Point2f(0, size.y) };
        var H = Cv2.GetPerspectiveTransform(src, dst);

        // Position the board by averaging points and aligning rotation/scale approximately
        // Convert image pixel positions to UI local positions under uiRoot
        Vector2[] imgPts = new[] {
            new Vector2(tl.X, tl.Y), new Vector2(tr.X, tr.Y), new Vector2(br.X, br.Y), new Vector2(bl.X, bl.Y)
        };
        // Convert image space (texture pixels) to RawImage rect local space: assume Surface RawImage matches texture size
        // We'll anchor the board at the centroid and set size to match the quad's width/height in UI space.
        Vector2 centroid = (imgPts[0] + imgPts[1] + imgPts[2] + imgPts[3]) * 0.25f;

        // Approximate width/height from edges
        float w = (Vector2.Distance(imgPts[0], imgPts[1]) + Vector2.Distance(imgPts[3], imgPts[2])) * 0.5f;
        float h = (Vector2.Distance(imgPts[0], imgPts[3]) + Vector2.Distance(imgPts[1], imgPts[2])) * 0.5f;
        float angle = Mathf.Atan2(imgPts[1].y - imgPts[0].y, imgPts[1].x - imgPts[0].x) * Mathf.Rad2Deg;

        // Translate image pixels to uiRoot local space assuming a 1:1 mapping with the Surface RawImage
        // If the Surface is scaled, you may need to convert via RectTransformUtility
        boardRect.anchoredPosition = centroid;
        boardRect.sizeDelta = new Vector2(w, h);
        boardRect.localRotation = Quaternion.Euler(0, 0, angle);
        boardAngleDeg = angle;
    }

    private bool TryDetectBall(Mat gray, out Point2f center)
    {
        center = default;
        try
        {
            using (var blurred = new Mat())
            {
                Cv2.GaussianBlur(gray, blurred, new Size(9, 9), 2);
                CircleSegment[] circles = Cv2.HoughCircles(
                    blurred,
                    HoughMethods.Gradient,
                    dp: 1.5,
                    minDist: 30,
                    param1: 100,
                    param2: 30,
                    minRadius: 6,
                    maxRadius: 80);
                if (circles != null && circles.Length > 0)
                {
                    // pick the largest circle (assume TT ball)
                    CircleSegment best = circles[0];
                    for (int i = 1; i < circles.Length; i++)
                        if (circles[i].Radius > best.Radius) best = circles[i];
                    center = best.Center;
                    return true;
                }
            }
        }
        catch (System.Exception)
        {
            // ignore
        }
        return false;
    }

    private void TryMarkByBall(Point2f imgCenter, int texW, int texH)
    {
        if (spawnedBoard == null) return;

        // convert image pixel center to uiRoot local (assuming center pivot and 1:1 size with texture)
        Vector2 boardSpaceCenter = new Vector2(imgCenter.X - texW * 0.5f, imgCenter.Y - texH * 0.5f);

        // relocate into board local by subtracting anchored position and un-rotating
        Vector2 v = boardSpaceCenter - spawnedBoard.anchoredPosition;
        float ang = -boardAngleDeg * Mathf.Deg2Rad;
        float cos = Mathf.Cos(ang), sin = Mathf.Sin(ang);
        Vector2 local = new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);

        // shift origin to top-left style inside the board rectangle
        Vector2 size = spawnedBoard.sizeDelta;
        Vector2 p = local + size * 0.5f;
        if (p.x < 0 || p.y < 0 || p.x > size.x || p.y > size.y) return; // outside

        int col = Mathf.Clamp(Mathf.FloorToInt(p.x / (size.x / 3f)), 0, 2);
        int row = Mathf.Clamp(Mathf.FloorToInt(p.y / (size.y / 3f)), 0, 2);

        // Ask the board to place mark at row/col (if present)
        spawnedBoard.gameObject.SendMessage("PlaceMarkAt", new Vector2Int(row, col), SendMessageOptions.DontRequireReceiver);
    }
    private Vector2[] tovector2(Point[] points)
    {
        vectorList = new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            vectorList[i] = new Vector2(points[i].X, points[i].Y);
        }
        return vectorList;
    }

    public void drawContour(Mat img, Scalar color, int thickness, Point[] points)
    {
        for (int i = 1; i < points.Length; i++)
        {
            Cv2.Line(img, points[i - 1], points[i], color, thickness);
        }
        Cv2.Line(img, points[points.Length - 1], points[0], color, thickness);
    }
}
