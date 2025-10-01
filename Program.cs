namespace Practice{
    public class Program{
        static void Main(string[] args){
            int width = 1920;
            int height = 1080;
            string? title = "Practice";

            Window newGame = new(width,height,title);

            using(newGame){
                newGame.Run();
            }
        }
    }
}
