using DBAtsiskaitymas;
using DBAtsiskaitymas.Contexts;
using DBAtsiskaitymas.Models;

public class Program
{
    private static void Main(string[] args)
    {
        var controler = new Controller();
        controler.InitiateController();

    }
}
