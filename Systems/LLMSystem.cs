using Terraria.ModLoader;

namespace BestFriendsFarm.Systems
{
    public class LLMSystem : ModSystem
    {
        // The URL of your running llama-server instance.
        // You can change this if you run the server on a different port or machine.
        public static string LLMServerUrl { get; } = "http://127.0.0.1:8080";
    }
}