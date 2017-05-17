
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;

/**
 * Bring data on patient samples from the diagnosis machine to the laboratory with enough molecules to produce medicine!
 **/
public class Player
{
    static void Main(string[] args)
    {
        MyHero myHero = new MyHero();


        string[] inputs;
        int projectCount = int.Parse(Console.ReadLine());
        for (int i = 0; i < projectCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int a = int.Parse(inputs[0]);
            int b = int.Parse(inputs[1]);
            int c = int.Parse(inputs[2]);
            int d = int.Parse(inputs[3]);
            int e = int.Parse(inputs[4]);
        }

        // game loop
        while (true)
        {
            Robot[] robots = new Robot[2];

            for (int i = 0; i < 2; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                string target = inputs[0];
                int eta = int.Parse(inputs[1]);
                int score = int.Parse(inputs[2]);
                int storageA = int.Parse(inputs[3]);
                int storageB = int.Parse(inputs[4]);
                int storageC = int.Parse(inputs[5]);
                int storageD = int.Parse(inputs[6]);
                int storageE = int.Parse(inputs[7]);
                int expertiseA = int.Parse(inputs[8]);
                int expertiseB = int.Parse(inputs[9]);
                int expertiseC = int.Parse(inputs[10]);
                int expertiseD = int.Parse(inputs[11]);
                int expertiseE = int.Parse(inputs[12]);

                Robot robot = new Robot(inputs);
                robots[i] = robot;
            }
            inputs = Console.ReadLine().Split(' ');
            int availableA = int.Parse(inputs[0]);
            int availableB = int.Parse(inputs[1]);
            int availableC = int.Parse(inputs[2]);
            int availableD = int.Parse(inputs[3]);
            int availableE = int.Parse(inputs[4]);
            int sampleCount = int.Parse(Console.ReadLine());

            Sample[] samples = new Sample[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int sampleId = int.Parse(inputs[0]);
                int carriedBy = int.Parse(inputs[1]);
                int rank = int.Parse(inputs[2]);
                string expertiseGain = inputs[3];
                int health = int.Parse(inputs[4]);
                int costA = int.Parse(inputs[5]);
                int costB = int.Parse(inputs[6]);
                int costC = int.Parse(inputs[7]);
                int costD = int.Parse(inputs[8]);
                int costE = int.Parse(inputs[9]);

                Sample sample = new Sample(inputs);
                samples[i] = sample;
            }

            Game game = new Game(robots, samples);
            string myAction = myHero.Think(game);
            Console.WriteLine(myAction);
        }
    }
}

public class MyHero
{
    public enum RobotCmd
    {
        None = -1,
        Collect,
        Gather,
        Produce
    }

    private static Random RNG = new Random();

    private int targetSampleID = -1;

    public string Think(Game game)
    {
        Sample targetSample = game.samples.FirstOrDefault(s => s!=null && s.SampleId == targetSampleID);
        Robot myRobot = game.robots[0];
        Robot enemyRobot = game.robots[1];

        string command = "";

        if (myRobot.HasASample == false)
        {
            command = ObtaintSample(myRobot);
            command += " - Obtaining -";
        }
        else if (targetSample == null)
        {
            this.targetSampleID = SelectTargetSample(myRobot).SampleId;
            command = "GOTO " + Modules.DIAGNOSIS;
            command += " - Selecting -";
        }
        else if (targetSample.WasDiagnosed == false)
        {
            targetSample.Cost.ToList().ForEach(s => Console.Error.Write(s+" "));
            command = "connect " + targetSample.SampleId;
            command += " - Diagnosing -";
        }
        else if (AllMaterialsAcquired(myRobot, targetSample) == false)
        {
            targetSample.Cost.ToList().ForEach(s => Console.Error.Write(s+" "));
            command = ObtainMolecules(myRobot, targetSample);
            command += " - Progressing -";
        }
        else
        {
            command = ProduceSample(myRobot, targetSample);
        }
        return command;
    }

    public Sample SelectTargetSample(Robot robot)
    {
        return robot.Samples.FirstOrDefault(s => s != null);
    }

    public string ObtaintSample(Robot robot)
    {
        string command = "";

        if (robot.Target != Modules.SAMPLES.ToString())
        {
            command = "goto " + Modules.SAMPLES;
        }
        else
        {
            command = "connect " + (RNG.Next(1) + 2);
        }
        return command;
    }

    public string ObtainMolecules(Robot myRobot, Sample sample)
    {
        string command = "";
        if (myRobot.Target != Modules.MOLECULES.ToString())
        {
            command = "GOTO " + Modules.MOLECULES;
        }
        else
        {
            if (sample.WasDiagnosed == false)
            {
                command = "connect " + sample.SampleId;
            }
            else if(AllMaterialsAcquired(myRobot, sample) == false)
            {
                foreach (var moleculeType in Enum.GetValues(typeof(MoleculeType)))
                {
                    int available = myRobot.Storage[(int) moleculeType];
                    int needed = sample.Cost[(int) moleculeType];
                    if (available < needed)
                    {
                        command = "connect "+ moleculeType;
                        command += " "+available + "/" + needed;
                        command += " target id is " +sample.SampleId;
                        break;
                    }
                }
            }
        }
        return command;
    }

    public string ProduceSample(Robot myRobot, Sample sample)
    {
        string command = "";
        if (myRobot.Target != Modules.LABORATORY.ToString())
        {
            command = "goto " + Modules.LABORATORY;
        }
        else
        {
            command = "connect " + sample.SampleId;
            targetSampleID = -1;
        }
        return command;
    }

    public bool AllMaterialsAcquired(Robot robot, Sample sample)
    {
        bool result = true;
        result = result && robot.StorageA >= sample.CostA;
        result = result && robot.StorageB >= sample.CostB;
        result = result && robot.StorageC >= sample.CostC;
        result = result && robot.StorageD >= sample.CostD;
        result = result && robot.StorageE >= sample.CostE;
        return result;
    }

}

public enum MoleculeType
{
    A = 0,
    B,
    C,
    D,
    E
}

public class Game
{

    public Robot[] robots;
    public Sample[] samples;

    public Game(Robot[] robots, Sample[] samples)
    {
        this.robots = robots;
        this.samples = samples;

        robots[0].Samples = samples.Where(s => s.CarriedBy == 0).ToArray();
        robots[1].Samples = samples.Where(s => s.CarriedBy == 1).ToArray();
    }
}

public class Robot
{
    public Sample[] Samples { get; set; } = new Sample[3];

    public bool HasASample => Samples.Count(s => s != null) > 0;

    public string Target { get; }
    public int Eta { get; }
    public int Score { get; }
    public int StorageA { get; }
    public int StorageB { get; }
    public int StorageC { get; }
    public int StorageD { get; }
    public int StorageE { get; }
    public int ExpertiseA { get; }
    public int ExpertiseB { get; }
    public int ExpertiseC { get; }
    public int ExpertiseD { get; }
    public int ExpertiseE { get; }

    public int[] Storage { get; }

    public Robot(string[] inputs)
    {
        Target = inputs[0];
        Eta = int.Parse(inputs[1]);
        Score = int.Parse(inputs[2]);
        StorageA = int.Parse(inputs[3]);
        StorageB = int.Parse(inputs[4]);
        StorageC = int.Parse(inputs[5]);
        StorageD = int.Parse(inputs[6]);
        StorageE = int.Parse(inputs[7]);
        ExpertiseA = int.Parse(inputs[8]);
        ExpertiseB = int.Parse(inputs[9]);
        ExpertiseC = int.Parse(inputs[10]);
        ExpertiseD = int.Parse(inputs[11]);
        ExpertiseE = int.Parse(inputs[12]);

        Storage = new int[] {StorageA, StorageB, StorageC, StorageD, StorageE };
    }
}


public class Sample
{
    public int SampleId { get; }
    public int CarriedBy { get; }
    public int Rank { get; }
    public string ExpertiseGain { get; }
    public int Health { get; }
    public int CostA { get; }
    public int CostB { get; }
    public int CostC { get; }
    public int CostD { get; }
    public int CostE { get; }

    public int[] Cost { get; }

    public bool WasDiagnosed => CostA + CostB + CostC + CostD + CostE > 0;

    public Sample(string[] inputs)
    {
        SampleId = int.Parse(inputs[0]);
        CarriedBy = int.Parse(inputs[1]);
        Rank = int.Parse(inputs[2]);
        ExpertiseGain = inputs[3];
        Health = int.Parse(inputs[4]);
        CostA = int.Parse(inputs[5]);
        CostB = int.Parse(inputs[6]);
        CostC = int.Parse(inputs[7]);
        CostD = int.Parse(inputs[8]);
        CostE = int.Parse(inputs[9]);

        Cost = new int[]{CostA, CostB, CostC, CostD, CostE};
    }


}


public enum Modules
{
    LABORATORY = 0,
    MOLECULES,
    DIAGNOSIS,
    SAMPLES
}
