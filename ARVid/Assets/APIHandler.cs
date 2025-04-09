using UnityEngine;
using Unity.Barracuda;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System;
using System.Threading.Tasks;

public class APIHandler : MonoBehaviour
{
    public NNModel treadmillIntensityModel;
    public NNModel ellipticalIntensityModel;
    public NNModel rowIntensityModel;
    public NNModel machineModel;

    private Model runtimeModel;
    private IWorker worker;

    void Start()
    {
        runtimeModel = ModelLoader.Load(treadmillIntensityModel);
        worker = WorkerFactory.CreateWorker(runtimeModel);
    }

    public async Task Get(string model, List<(double X, double Y, double Z)> recentHeadPositionData)
    {
        var queryParams = "";
        foreach (var item in recentHeadPositionData)
        {
            queryParams += $"&X={item.X}&Y={item.Y}&Z={item.Z}";
        }

        var url = $"http://127.0.0.1:8000/run_inference?{model}?{queryParams.TrimStart('&')}";

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Response: " + jsonString);
                }
                else
                {
                    Console.WriteLine($"Request failed with status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Request error: {ex.Message}");
            }
        }
    }

    public void RunInference(float[] features)
    {
        Tensor inputTensor = new Tensor(1, features.Length, features);

        Tensor outputTensor = worker.Execute(inputTensor).PeekOutput();

        float[] results = outputTensor.ToReadOnlyArray();
        Debug.Log("Inference Output: " + string.Join(", ", results));

        inputTensor.Dispose();
        outputTensor.Dispose();
    }

    void OnDestroy()
    {
        worker.Dispose();
    }
}