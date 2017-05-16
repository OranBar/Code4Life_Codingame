
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Policy;
using System.Security.Principal;

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

    private Sample targetSample;

    public string Think(Game game)
    {
        Robot myRobot = game.robots[0];
        Robot enemyRobot = game.robots[1];
        RobotCmd cmd;

        string command = "";


        if (targetSample == null)
        {
            if (myRobot.Samples.Count(s => s != null) == 0)
            {
                if (myRobot.Target != Modules.SAMPLES.ToString())
                {
                    command = "GOTO " + Modules.SAMPLES;
                }
                else
                {
                    cmd = RobotCmd.Collect;
                    command = "connect " + (int) (RNG.Next(2) + 1);
                }
            }
            else
            {
                targetSample = myRobot.Samples.First(s => s != null);
                command = "GOTO " + Modules.MOLECULES;
            }
        }
        else if (AllMaterialsAcquired(myRobot, targetSample) == false)
        {
            cmd = RobotCmd.Gather;

            if (myRobot.Target != Modules.MOLECULES.ToString())
            {
                command = "GOTO " + Modules.MOLECULES;
            }
            else
            {
                foreach (var moleculeType in Enum.GetValues(typeof(MoleculeType)))
                {
                    int available = myRobot.Storage[(int) moleculeType];
                    int needed = targetSample.Cost[(int) moleculeType];
                    if (available <= needed)
                    {
                        command = "connect "+ moleculeType;
                        command += available + "/" + needed;
                        command += " target id is " +targetSample.SampleId;
                    }
                }
//
//
//                if (myRobot.StorageA <= targetSample.CostA)
//                {
//                    command = "connect A ";
//                    command += myRobot.StorageA + " / " + targetSample.CostA;
//                    command += " target id is " +targetSample.SampleId;
//                }
//                else
//                if (myRobot.StorageB <= targetSample.CostB)
//                {
//                    command = "connect B ";
//                    command += myRobot.StorageB + " / " + targetSample.CostB;
//                    command += " target id is " +targetSample.SampleId;
//                }
//                else
//                if (myRobot.StorageC <= targetSample.CostC)
//                {
//                    command = "connect C ";
//                    command += myRobot.StorageC + " / " + targetSample.CostC;
//                    command += " target id is " +targetSample.SampleId;
//
//                }
//                else
//                if (myRobot.StorageD <= targetSample.CostD)
//                {
//                    command = "connect D ";
//                    command += myRobot.StorageD + " / " + targetSample.CostD;
//                    command += " target id is " +targetSample.SampleId;
//                }
//                else
//                if (myRobot.StorageE <= targetSample.CostE)
//                {
//                    command = "connect E ";
//                    command += myRobot.StorageE + " / " + targetSample.CostE;
//                    command += " target id is " +targetSample.SampleId;
//                }
            }
        } else
        {
            if (myRobot.Target != Modules.LABORATORY.ToString())
            {
                command = "GOTO " + Modules.LABORATORY;
            }
            else
            {
                command = "connect " + targetSample.SampleId;
                targetSample = null;
            }
            cmd = RobotCmd.Produce;
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

        robots[0].Samples = samples.Where(s => s.CarriedBy == 1).ToArray();
        robots[1].Samples = samples.Where(s => s.CarriedBy == 0).ToArray();
    }
}

public class Robot
{
    public Sample[] Samples { get; set; } = new Sample[3];

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
