using Terraria.ModLoader;

public class ToggleLearningModeCommand : ModCommand
{
    private bool isLearningModeActive = false;

    public override CommandType Type => CommandType.Chat; // Comando de chat do jogo

    // Define o nome do comando no jogo
    public override string Command => "togglelearn";

    // Descrição do comando
    public override string Usage => "/togglelearn";

    // O método que será chamado quando o comando for executado
    public override void Action(CommandCaller caller, string input, string[] args)
    {
        ToggleLearningMode(); // Alterna o estado do modo de aprendizado
        caller.Reply($"Modo de aprendizado {(isLearningModeActive ? "ativado" : "desativado")}.");
    }

    // Alterna o modo de aprendizado
    public void ToggleLearningMode()
    {
        isLearningModeActive = !isLearningModeActive;
    }

    // Verifica se o modo de aprendizado está ativo
    public bool IsLearningModeActive()
    {
        return isLearningModeActive;
    }
}
