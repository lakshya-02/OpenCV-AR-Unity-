using OpenCvSharp.Demo;
using OpenCvSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CounterFinder : WebCamera
{
    [SerializeField] private FlipMode ImageFlip;
    [SerializeField] private float Threshold = 96.4f;
    [SerializeField] private bool ShowProcessingImage = true;
    [SerializeField] private float CurveAccuracy = 10f;
    [SerializeField] private float MinArea = 5000f;
    [SerializeField] private PolygonCollider2D polygonCollider;

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
        Cv2.Threshold(processImage, processImage, Threshold, 255, ThresholdTypes.Binary);
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
    private Vector2[] tovector2(Point[] points)
    {
        Vector2[] vectorList = new Vector2[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            vectorList[i] = new Vector2(points[i].X, points[i].Y);
        }
        return vectorList;
    }

    public void drawContour(Mat img, Scalar color, int thickness, Point[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            int prevIndex = (i == 0) ? points.Length - 1 : i - 1;
            Cv2.Line(img, points[prevIndex], points[i], color, thickness);
        }
    }
}
