using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>
/// Initiates a neural network from a topology and a seed
/// </summary>

public class NeuralNetwork //(System.UInt32[] Topology, System.Int32? Seed = 0)
{
    // Return the topology in the form of an array
    //public uint[] -- if system.uint doesnt compile
    public System.UInt32[] Topology
    {
        get
        {
            System.UInt32[] Result = new System.UInt32[TheTopology.Count];
            TheTopology.CopyTo(Result, 0);
            return Result;
        }
    }

    ReadOnlyCollection<System.UInt32> TheTopology;// Contains the topology of the neural network
    NeuralSection[] Sections;// Contains all of the sections of the neural network
    //public static List<double> dataSet = new List<double>();//tk


    System.Random TheRandomiser; // The random instance used to mutate the neural network

    private class NeuralSection
    {
        //Represents weights - Neuron i = input, Neuron j = output
        private double[][] Weights; // Contains all the weighting biases within [i][j]

        private System.Random TheRandomiser;

        /// <summary>
        /// Initiate a NeuralSection from a topology and a seed.
        /// </summary>
        /// <param name="InputCount">Number of input neurons</param>
        /// <param name="OutputCount">Number of output neurons</param>
        /// <param name="Randomiser">Random instance of the neural network</param>
        public NeuralSection(System.UInt32 InputCount, System.UInt32 OutputCount, System.Random Randomiser)
        {
            // Validation checks
            if (InputCount == 0)
                throw new System.ArgumentException
                    ("You cannot create a neural layer with no input neurons.", "InputCount");
            else if (OutputCount == 0)
                throw new System.ArgumentException
                ("You cannot create a Neural Layer with no output neurons.", "OutputCount");
            else if (Randomiser == null)
                throw new System.ArgumentException("The randomizer cannot be set to null.", "Randomiser");

            // Set randomiser
            TheRandomiser = Randomiser;

            // Initialise weights in array
            Weights = new double[InputCount + 1][]; // +1 for bias neuron

            for (int i = 0; i < Weights.Length; i++)
            {
                Weights[i] = new double[OutputCount];
            }

            // Set random weights
            for (int i = 0; i < Weights.Length; i++)
            {
                for (int j = 0; j < Weights[i].Length; j++)
                {
                    Weights[i][j] = TheRandomiser.NextDouble() - 0.5f;
                }
            }
        }

        /// <summary>
        /// Initiates a deep-copy of the neural section already provided
        /// </summary>
        /// <param name="Main"></param>
        public NeuralSection(NeuralSection Main)
        {
            //Set randomiser
            TheRandomiser = Main.TheRandomiser;

            // Initialise weights
            Weights = new double[Main.Weights.Length][];

            for (int i = 0; i < Weights.Length; i++)
            {
                Weights[i] = new double[Main.Weights[0].Length];
            }

            // Set weights
            for (int i = 0; i < Weights.Length; i++)
            {
                for (int j = 0; j < Weights[i].Length; j++)
                {
                    Weights[i][j] = Main.Weights[i][j];
                }
            }
        }

        /// <summary>
        /// Feed input through the neural section and get the output
        /// </summary>
        /// <param name="neuralInput">Values of input neurons</param>
        /// <returns>Values of output neurons after propagation</returns>
        public double[] FeedForward(double[] neuralInput)
        {
            //Validation checks
            if (neuralInput == null)
                throw new System.ArgumentException("The input array cannot be set to null.", "Input");
            else if (neuralInput.Length != Weights.Length - 1)
                throw new System.ArgumentException("The input array's length does not match the number of neurons in the input layer.", "Input");

            // Initialise output array
            double[] neuralOutput = new double[Weights[0].Length];

            // Calculate value
            for (int i = 0; i < Weights.Length; i++)
            {
                for (int j = 0; j < Weights[i].Length; j++)
                {
                    // Add weight to dataset
                    //dataSet.Add(Weights[i][j]);//tk
                    if (i == Weights.Length - 1)// If is bias neuron
                    {
                        neuralOutput[j] += Weights[i][j];// Then value of neuron is equal to 1
                    }
                    else
                    {
                        neuralOutput[j] += Weights[i][j] * neuralInput[i];
                    }
                }
            }
            // Clear weights
            //dataSet.Clear();

            // Apply activation function
            for (int i = 0; i < neuralOutput.Length; i++)
            {
                neuralOutput[i] = ReLu(neuralOutput[i]);//ReLu activation function
            }

            return neuralOutput;
        }

        /// <summary>
        /// Mutate the NeuralSection
        /// </summary>
        /// <param name="MutationProbability"></param>The mutation probability - Weight of mutations: Ranged 0 - 1
        /// <param name="MutationAmount"></param>Maximum amount a mutated weight will change
        public void Mutate(double MutationProbability, double MutationAmount)
        {
            for (int i = 0; i < Weights.Length; i++)
            {
                for (int j = 0; j < Weights[i].Length; j++)
                {
                    if (TheRandomiser.NextDouble() < MutationProbability)
                        Weights[i][j] = TheRandomiser.NextDouble() *
                                        (MutationAmount * 2) - MutationAmount;
                }
            }
        }

        //Put a double through the function to provide the activation function, returning x
        private double ReLu(double x)
        {
            if (x >= 0)
            {
                return x;
            }
            else
            {
                return x / 20;
            }
        }

    }

    public NeuralNetwork(System.UInt32[] Topology, System.Int32? Seed = 0)
    {
        // Validation checks
        if (Topology.Length < 2)
            throw new System.ArgumentException("A Neural Network cannot contain less than 2 layers.", "Topology");

        for (int i = 0; i < Topology.Length; i++)
        {
            if (Topology[i] < 1)
                throw new System.ArgumentException("A single layer of neurons must contain at least, one neuron.", "Topology");
        }

        //Initialise randomiser
        if (Seed.HasValue)
        {
            TheRandomiser = new System.Random(Seed.Value);
        }
        else
        {
            TheRandomiser = new System.Random();
        }

        // Set Topology
        TheTopology = new List<uint>(Topology).AsReadOnly();

        // Initialise sections
        Sections = new NeuralSection[TheTopology.Count - 1];

        // Set the sections
        for (int i = 0; i < Sections.Length; i++)
        {
            Sections[i] = new NeuralSection(TheTopology[i], TheTopology[i + 1], TheRandomiser);
        }
    }

    /// <summary>
    /// Initiate an independent deep copy of the nerual network (maybe call this copy?)
    /// </summary>
    /// <param name="Main"></param>
    public NeuralNetwork(NeuralNetwork Main)
    {
        // Initialise randomiser
        TheRandomiser = new System.Random(Main.TheRandomiser.Next());

        // Set topology
        TheTopology = Main.TheTopology;

        Sections = new NeuralSection[TheTopology.Count - 1];

        // Initialise Sections
        for (int i = 0; i < Sections.Length; i++)
        {
            Sections[i] = new NeuralSection(Main.Sections[i]);
        }
    }


    /// <summary>
    /// Feed input through the neural network and get the output
    /// </summary>
    /// <param name="neuralInput"></param>
    /// <returns></returns>
    public double[] FeedForward(double[] neuralInput)
    {
        // Validation checks
        if (neuralInput == null)
            throw new System.ArgumentException("The input array cannot be set to null.", "Input");
        else if (neuralInput.Length != TheTopology[0])
            throw new System.ArgumentException
                ("The input array's length does not match the number of neurons in the input layer.", "Input");

        double[] neuralOutput = neuralInput;

        // Feed values through all sections
        for (int i = 0; i < Sections.Length; i++)
        {
            neuralOutput = Sections[i].FeedForward(neuralOutput);
        }

        return neuralOutput;
    }


    /// <summary>
    /// Mutate the neural network (Maybe add to changeable variables)
    /// </summary>
    /// <param name="MutationProbability"></param>Probability that a weight is going to be mutated. Ranges: (0 - 1)
    /// <param name="MutationAmount"></param>Maximum amount a mutated weight will change
    public void Mutate(double MutationProbability = 0.3, double MutationAmount = 2.0)
    {
        // Mutate each section
        for (int i = 0; i < Sections.Length; i++)
        {
            Sections[i].Mutate(MutationProbability, MutationAmount);
        }

    }
    /*tk
    public string ParseData()
    {
        string networkData = "";
        

        for (int i = 0; i < dataSet.Count; i++)
        {
            networkData += dataSet[i].ToString() + ",";
        }

        return networkData;
    }*/
}
