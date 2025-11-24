// Verifica se o modo debug foi ativado
bool debugMode = args.Length > 0 && args[0].ToLower() == "--debug";

using var game = new MinimalRoutes.MainGame(debugMode);
game.Run();
