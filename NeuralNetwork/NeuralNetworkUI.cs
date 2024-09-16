using System;

namespace MinerSocietyMod014.NeuralNetwork
{
    public class NeuralNetworkUI
    {
        private NeuralNetwork neuralNetwork;

        public NeuralNetworkUI()
        {
            // Inicializa a rede neural
            neuralNetwork = new NeuralNetwork();
        }

        // Método para carregar e treinar o modelo de rede neural
        public void LoadTrainingData(string path)
        {
            neuralNetwork.TrainModel(path); // Treina o modelo de rede neural
            Console.WriteLine("Modelo de rede neural treinado.");
        }

        // Método para visualizar o progresso do treinamento
        public void ShowTrainingProgress()
        {
            neuralNetwork.VisualizeTrainingProgress(); // Mostra o progresso do treinamento
        }

        // Método para fazer uma predição e visualizar a decisão do bot de minerar ou não
        public void PredictMiningAction(MiningData data)
        {
            bool shouldMine = neuralNetwork.Predict(data);
            Console.WriteLine(shouldMine ? "O bot vai minerar o bloco." : "O bot vai ignorar o bloco.");
        }
    }
}
