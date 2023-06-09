﻿using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using YOLOv4MLNet.DataStructures;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;

namespace YOLOv4MLNet
{
    /// <summary>
    /// Represents an object recognition class that utilizes YOLOv4 model for detection.
    /// </summary>
    public class ObjectRecognition
    {

        /// <summary>
        /// Path to the onnx yolov4 model
        /// <see cref="https://github.com/onnx/models/tree/main/vision/object_detection_segmentation/yolov4"/>
        /// </summary>
        const string modelPath = @"C:\Users\Pep\source\repos\YOLOv4MLNet\YOLOv4MLNet\DataStructures\Model\yolov4.onnx";

        static readonly string[] classesNames = new string[] { "person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train", "truck", "boat", "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed", "diningtable", "toilet", "tvmonitor", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush" };

        PredictionEngine<YoloV4BitmapData, YoloV4Prediction> predictionEngine = null;
        
        /// <summary>
        /// Trains the model.
        /// </summary>
        public void trainModel()
        {
            MLContext mlContext = new MLContext();

            // Define scoring pipeline
            var pipeline = mlContext.Transforms.ResizeImages(inputColumnName: "bitmap", outputColumnName: "input_1:0", imageWidth: 416, imageHeight: 416, resizing: ResizingKind.IsoPad)
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input_1:0", scaleImage: 1f / 255f, interleavePixelColors: true))
                .Append(mlContext.Transforms.ApplyOnnxModel(
                    shapeDictionary: new Dictionary<string, int[]>()
                    {
                        { "input_1:0", new[] { 1, 416, 416, 3 } },
                        { "Identity:0", new[] { 1, 52, 52, 3, 85 } },
                        { "Identity_1:0", new[] { 1, 26, 26, 3, 85 } },
                        { "Identity_2:0", new[] { 1, 13, 13, 3, 85 } },
                    },
                    inputColumnNames: new[]
                    {
                        "input_1:0"
                    },
                    outputColumnNames: new[]
                    {
                        "Identity:0",
                        "Identity_1:0",
                        "Identity_2:0"
                    },
                    modelFile: modelPath, recursionLimit: 100));

            // Fit on empty list to obtain input data schema
            var model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));

            // Create prediction engine
            predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);
        }
        /// <summary>
        /// Process image
        /// </summary>
        /// <param name="ImageToProcess"> byte representation of an image</param>
        /// <returns>YoloV4Result class that represents a result of object recognition</returns>
        public IReadOnlyList<YoloV4Result> ProcessImage(byte[] ImageToProcess)
        {

            var sw = new Stopwatch();
            sw.Start();
            using (var ms = new MemoryStream(ImageToProcess))
            {
                var bitmap = new Bitmap(ms);
                // predict
                var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
                sw.Stop();
                Console.WriteLine($"Done in {sw.ElapsedMilliseconds}ms.");
                return predict.GetResults(classesNames, 0.3f, 0.7f);
            }

        }
    }
}
