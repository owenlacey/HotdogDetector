﻿// This file was auto-generated by ML.NET Model Builder. 
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ML;
using Microsoft.OpenApi.Models;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using MLModel2 = HotdogDetector_Api.MLModel1;
using Microsoft.AspNetCore.Http;
using Microsoft.ML;
using System.Collections.Generic;

// Configure app
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPredictionEnginePool<MLModel1.ModelInput, MLModel1.ModelOutput>()
    .FromFile("MLModel1.zip");
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Description = "Docs for my API", Version = "v1" });
});
var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

// Define prediction route & handler
app.MapPost("/predict",
    async (PredictionEnginePool<MLModel1.ModelInput, MLModel1.ModelOutput> predictionEnginePool, HttpRequest request) =>
    {
        var file = request.Form.Files[0];
        var stream = new MemoryStream();
        file.CopyTo(stream);
        var input = new MLModel1.ModelInput()
        {
            ImageSource = stream.ToArray(),
        };
        var result = predictionEnginePool.Predict(input);

        return result;
    })
    .Accepts<IFormFile>("multipart/form-data");

// Define prediction route & handler
app.MapPost("/retrain",
    async () =>
    {
        var context = new MLContext();
        var trainingExamples = new List<MLModel1.ModelInput>();
        var hotdogFileNames = Directory.GetFiles(@"C:\src\Hainton.MachineLearning\HotdogDetector\Data\Hotdog");
        foreach (var hotdogFileName in hotdogFileNames)
        {
            var fileBytes = await File.ReadAllBytesAsync(hotdogFileName);
            trainingExamples.Add(new MLModel1.ModelInput
            {
                ImageSource = fileBytes,
                Label = "Hotdog"
            });
        }
        var notHotdogFileNames = Directory.GetFiles(@"C:\src\Hainton.MachineLearning\HotdogDetector\Data\NotHotdog");
        foreach (var notHotdogFileName in notHotdogFileNames)
        {
            var fileBytes = await File.ReadAllBytesAsync(notHotdogFileName);
            trainingExamples.Add(new MLModel1.ModelInput
            {
                ImageSource = fileBytes,
                Label = "NotHotdog"
            });
        }
        var imageDataView = context.Data.LoadFromEnumerable(trainingExamples);

        var trainedModel = MLModel2.RetrainPipeline(context, imageDataView);

        context.Model.Save(trainedModel, imageDataView.Schema, Path.GetFullPath("MLModel1.zip"));

    });

app.UseCors(opts => opts.AllowAnyOrigin());

// Run app
app.Run();