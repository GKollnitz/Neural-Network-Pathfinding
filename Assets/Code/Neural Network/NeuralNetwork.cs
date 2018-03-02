using System.Collections.Generic;
using System;
using UnityEngine;


public class NeuralNetwork : IComparable<NeuralNetwork>
{
    private int[] layers; //Layers
    private float[][] neurons;  //Neuron Matrix
    public float[][][] weights; //Weights Matrix
    private float knowledge; //Knowledge / Fitness

    //Constructors for the Network
    public NeuralNetwork(int[] layers)
    {
        this.layers = new int[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }
        InitNeurons();
        InitWeights();
    }

    public NeuralNetwork(NeuralNetwork copyNetwork)
    {
        this.layers = new int[copyNetwork.layers.Length];
        for (int i = 0; i < copyNetwork.layers.Length; i++)
        {
            this.layers[i] = copyNetwork.layers[i];
        }

        InitNeurons();
        InitWeights();
        CopyWeights(copyNetwork.weights);
    }

    public void AddKnowledge(float k)
    {
        knowledge += k;
    }

    public void SetKnowledge(float k)
    {
        knowledge = k;
    }

    public float GetKnowledge()
    {
        return knowledge;
    }

    //Get weights of network, depending on layernumber
    public float[] GetWeights(int layerNumber)
    {
        if (layerNumber == 1)
        {
            float[] array = new float[layers[0] * layers[1]];
            for (int i = 0; i < layers[0] * layers[1]; i = i + layers[0])
            {
                array[i] = weights[0][i / layers[0]][0];
                array[i + 1] = weights[0][i / layers[0]][1];
            }
            return array;
        }
        else if (layerNumber == 2)
        {
            float[] array = new float[layers[1] * layers[2]];
            for (int i = 0; i < layers[1] * layers[2]; i = i + layers[1])
            {
                array[i] = weights[1][i / layers[1]][0];
                array[i + 1] = weights[1][i / layers[1]][1];
                array[i + 2] = weights[1][i / layers[1]][2];
                array[i + 3] = weights[1][i / layers[1]][3];
            }
            return array;
        }
        else if (layerNumber == 3)
        {
            float[] array = new float[layers[2] * layers[3]];
            for (int i = 0; i < layers[2] * layers[3]; i = i + layers[2])
            {
                array[i] = weights[2][i / layers[2]][0];
                array[i + 1] = weights[2][i / layers[2]][1];
                array[i + 2] = weights[2][i / layers[2]][2];
                array[i + 3] = weights[2][i / layers[2]][3];
            }
            return array;
        }
        else
        {
            return new float[0];
        }

    }

    //Set weights of network
    public void SetWeights(float[] newWeights)
    {
        int weightsSet = 0;
        for (int i = 0; i < layers[1]; i++)
        {
            for (int j = 0; j < layers[0]; j++)
            {
                weights[0][i][j] = newWeights[weightsSet];
                weightsSet++;
            }
        }
        for (int i = 0; i < layers[2]; i++)
        {
            for (int j = 0; j < layers[1]; j++)
            {
                weights[1][i][j] = newWeights[weightsSet];
                weightsSet++;
            }
        }
        for (int i = 0; i < layers[3]; i++)
        {
            for (int j = 0; j < layers[2]; j++)
            {
                weights[2][i][j] = newWeights[weightsSet];
                weightsSet++;
            }
        }
    }

    private void CopyWeights(float[][][] copyWeights)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    weights[i][j][k] = copyWeights[i][j][k];
                }
            }
        }
    }

    //Create Neuron Matrix
    private void InitNeurons()
    {
        List<float[]> neuronsList = new List<float[]>();

        for (int i = 0; i < layers.Length; i++) //For each layer, add neurons
        {
            neuronsList.Add(new float[layers[i]]);
        }

        neurons = neuronsList.ToArray();
    }

    //Create Weights Matrix
    private void InitWeights()
    {
        List<float[][]> weightsList = new List<float[][]>();

        //Itterate over all neurons that have a weight connection
        for (int i = 1; i < layers.Length; i++) //For each layer, starting with 2nd layer, because the 1st is for inputs
        {
            List<float[]> layerWeightList = new List<float[]>();

            int neuronsInPreviousLayer = layers[i - 1];

            //Itterate over all neurons in this current layer
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float[] neuronWeights = new float[neuronsInPreviousLayer];

                //Itterate over all neurons in the previous layer and set the weights randomly between 0.5f and -0.5
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    neuronWeights[k] = UnityEngine.Random.Range(-0.5f, 0.5f);
                }

                layerWeightList.Add(neuronWeights);
            }

            weightsList.Add(layerWeightList.ToArray());
        }

        weights = weightsList.ToArray();
    }


    //Feed forward this neural network with a given input array
    public float[] FeedForward(float[] inputs)
    {
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
            
        }
        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {

                float value = 0;
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }

                neurons[i][j] = (float)Math.Tanh(value); //Hyperbolic tangent activation
            }
        }
        return neurons[neurons.Length - 1]; //return output layer
    }

    public void Mutate()
    {
        for (int i = 0; i < weights.Length; i++) //Itterate through all layers
        {
            for (int j = 0; j < weights[i].Length; j++) //Itterate through all neurons in the layer
            {
                for (int k = 0; k < weights[i][j].Length; k++) //Itterate through all weights from previous layer, connected to that neuron
                {
                    float weight = weights[i][j][k];


                    float rand = UnityEngine.Random.Range(0f, 1f) * 1000f;

                    //Chance to mutate is 0.8% for each weight at each new generation
                    if (rand <= 2f)
                    {
                        //Change from positive to negative or negative to positive
                        weight *= -1f;
                    }
                    else if (rand <= 4f)
                    {
                        //Pick a new random weight
                        weight = UnityEngine.Random.Range(-0.5f, 0.5f);
                    }
                    else if (rand <= 6f)
                    {
                        //Randomly increase by 0% to 100%
                        float factor = UnityEngine.Random.Range(0f, 1f) + 1f;
                        weight *= factor;
                    }
                    else if (rand <= 8f)
                    {
                        //Randomly decrease by 0% to 100%
                        float factor = UnityEngine.Random.Range(0f, 1f);
                        weight *= factor;
                    }
                    weights[i][j][k] = weight;
                }
            }
        }
    }

    //CompareTo method override to Sort() depending on knowledge
    public int CompareTo(NeuralNetwork other)
	{
		if (other == null) return 1;

        if (knowledge > other.knowledge)
            return 1;
        else if (knowledge < other.knowledge)
            return -1;
        else
            return 0;
	}
}
