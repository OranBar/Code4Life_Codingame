
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Bring data on patient samples from the diagnosis machine to the laboratory with enough molecules to produce medicine!
 **/
public class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        int projectCount = int.Parse(Console.ReadLine());
        ScienceProject[] projects = new ScienceProject[projectCount];
        Console.Error.WriteLine("My Science Projects");
        for (int i = 0; i < projectCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int a = int.Parse(inputs[0]);
            int b = int.Parse(inputs[1]);
            int c = int.Parse(inputs[2]);
            int d = int.Parse(inputs[3]);
            int e = int.Parse(inputs[4]);
            Console.Error.WriteLine($"Project {i} : {a} {b} {c} {d} {e}");
            projects[i] = new ScienceProject(a, b, c, d, e);
        }

        Gundam gundam = new Gundam(projects);

        // game loop
        while (true)
        {
            Robot[] robots = new Robot[2];

            for (int i = 0; i < 2; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                Robot robot = new Robot(inputs);
                robots[i] = robot;
            }

            inputs = Console.ReadLine().Split(' ');
            int availableA = int.Parse(inputs[0]);
            int availableB = int.Parse(inputs[1]);
            int availableC = int.Parse(inputs[2]);
            int availableD = int.Parse(inputs[3]);
            int availableE = int.Parse(inputs[4]);
            Dictionary<MoleculeType, int> availableMolecules = new Dictionary<MoleculeType, int>();
            availableMolecules[MoleculeType.A] = availableA;
            availableMolecules[MoleculeType.B] = availableB;
            availableMolecules[MoleculeType.C] = availableC;
            availableMolecules[MoleculeType.D] = availableD;
            availableMolecules[MoleculeType.E] = availableE;


            int sampleCount = int.Parse(Console.ReadLine());



            Sample[] samples = new Sample[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
//                int sampleId = int.Parse(inputs[0]);
//                int carriedBy = int.Parse(inputs[1]);
//                int rank = int.Parse(inputs[2]);
//                string expertiseGain = inputs[3];
//                int health = int.Parse(inputs[4]);
//                int costA = int.Parse(inputs[5]);
//                int costB = int.Parse(inputs[6]);
//                int costC = int.Parse(inputs[7]);
//                int costD = int.Parse(inputs[8]);
//                int costE = int.Parse(inputs[9]);
                Sample sample = new Sample(inputs);
                samples[i] = sample;
            }

            Game game = new Game(robots, samples, availableMolecules);
            string myAction = gundam.Think(game);
            Console.WriteLine(myAction);
        }
    }
}

public class Gundam
{
//    public enum RobotCmd
//    {
//        None = -1,
//        Collect,
//        Gather,
//        Produce
//    }

    private static Random RNG = new Random();

    private int targetSampleID = -1;
    private List<ScienceProject> MyProjects { get; }

    private bool refillSamples = true;

    private List<Sample> processedSamples = new List<Sample>();

    public Gundam(ScienceProject[] projects)
    {
        MyProjects = projects.ToList();
    }

    public string Think(Game game)
    {
        Sample targetSample = game.samples.FirstOrDefault(s => s.SampleId == targetSampleID);
        Robot myRobot = game.robots[0];
        Robot enemyRobot = game.robots[1];

        string command = "";

        if (myRobot.Samples.Length == 0)
        {
            Console.Error.WriteLine("Refilling");
            refillSamples = true;
        }
        else if (myRobot.Samples.Length == 3)
        {
            Console.Error.WriteLine("Stop Refilling");
            refillSamples = false;
        }

        if (refillSamples)
        {
            if (myRobot.Target != Modules.SAMPLES.ToString() || myRobot.Eta > 0)
            {
                return "goto " + Modules.SAMPLES;
            }

            command = GetASample(myRobot);
            command += " - Obtaining -";

            return command;
        }

        //From here on I assume I have a target sample
        if (myRobot.Samples.Count(s => s.WasDiagnosed == false) > 0)
        {
            Console.Error.WriteLine(targetSample);

            if (myRobot.Target != Modules.DIAGNOSIS.ToString() || myRobot.Eta > 0)
            {
                return "goto " + Modules.DIAGNOSIS;
            }

            command = DiagnoseAllSamples(myRobot);
            command += " - Diagnosing All-";

            Func<Robot, bool> allSamplesDiagoned = robot => robot.Samples.Count(s => s.WasDiagnosed == false) == 1;

            if (allSamplesDiagoned(myRobot))
            {
                SelectNewTargetSample(myRobot);
            }

        }
        else if (AllMaterialsAcquired(myRobot, targetSample) == false)
        {
            Console.Error.WriteLine(targetSample);

            if (myRobot.Target != Modules.MOLECULES.ToString() || myRobot.Eta > 0)
            {
                return "GOTO " + Modules.MOLECULES;
            }

            MoleculeType moleculeType = ChooseMoleculeToGather(myRobot, targetSample);
            command = ObtainMolecules(game, myRobot, moleculeType);
            command += " - Progressing -";
        }
        else
        {
            if (myRobot.Target != Modules.LABORATORY.ToString() || myRobot.Eta > 0)
            {
                return "goto " + Modules.LABORATORY;
            }

            command = ProduceSample(myRobot, targetSample);
            myRobot.Samples = myRobot.Samples.Where(s => s != targetSample).ToArray();
            SelectNewTargetSample(myRobot);

            Console.Error.WriteLine("Sample produced");
        }
        return command;
    }

    private void SelectNewTargetSample(Robot robot)
    {
        Sample newTargetSample = robot.Samples.FirstOrDefault(s => s.WasDiagnosed);
        this.targetSampleID = newTargetSample?.SampleId ?? -1;
    }

    public string GetASample(Robot robot)
    {
        string command = "";
        command = "connect 2";
//        Console.Error.WriteLine("Molecule expertise "+robot.moleculeExpertise.Values.Sum());
//        if (robot.moleculeExpertise.Values.Sum() <= 2)
//        {
//            command = "connect 1";
//        }
//        else
//        {
//        }
        return command;
    }
    //here

    public string DiagnoseAllSamples(Robot robot)
    {
        string command = "connect " + robot.Samples.First(s => s.WasDiagnosed == false).SampleId;
        return command;
    }

    private MoleculeType ChooseMoleculeToGather(Robot myRobot, Sample targetSample)
    {
        MoleculeType targetMolecule = MoleculeType.None;
        int needed = 0;
        foreach (MoleculeType moleculeType in MoleculeTypeEx.Enumerate())
        {
            int MoleculesNeeded = targetSample.MoleculesNeeded[moleculeType] - myRobot.moleculeExpertise[moleculeType];
            if (MoleculesNeeded <= myRobot.moleculesOwned[moleculeType])
            {
                continue;
            }

            if (MoleculesNeeded > needed)
            {
                targetMolecule = moleculeType;
                needed = targetSample.MoleculesNeeded[moleculeType];
            }
        }
        return targetMolecule;
    }

    public string ObtainMolecules(Game game, Robot myRobot, MoleculeType molecule)
    {
        string command = "wait";
        if (molecule == MoleculeType.None)
        {
            return command;
        }


        if (game.availableMolecules[molecule] > 0)
        {
            command = "connect " + molecule;
        }
        else
        {
            Console.Error.WriteLine("Am I really here?");
//            targetSampleID = new Random().Next(myRobot.Samples.Length);
        }

        return command;
    }

    public string ProduceSample(Robot myRobot, Sample sample)
    {
        targetSampleID = -1;
        processedSamples.Add(sample);
        return "connect " + sample.SampleId;
    }

    public bool AllMaterialsAcquired(Robot robot, Sample sample)
    {
        bool result = true;
        result = result && robot.StorageA + robot.ExpertiseA >= sample.CostA;
        result = result && robot.StorageB + robot.ExpertiseB >= sample.CostB;
        result = result && robot.StorageC + robot.ExpertiseC >= sample.CostC;
        result = result && robot.StorageD + robot.ExpertiseD >= sample.CostD;
        result = result && robot.StorageE + robot.ExpertiseE >= sample.CostE;
        return result;
    }

}

public enum MoleculeType
{
    None = -1,
    A = 0,
    B,
    C,
    D,
    E
}

public static class MoleculeTypeEx
{
    public static IEnumerable<MoleculeType> Enumerate()
    {
        return Enum.GetValues(typeof(MoleculeType)).Cast<MoleculeType>().Where(m => m != MoleculeType.None);
    }
}
public class Game
{

    public Robot[] robots;
    public Sample[] samples;
    public Dictionary<MoleculeType, int> availableMolecules;

    public Game(Robot[] robots, Sample[] samples, Dictionary<MoleculeType, int> availableMolecules)
    {
        this.robots = robots;
        this.samples = samples;
        this.availableMolecules = availableMolecules;

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
    public Dictionary<MoleculeType, int> moleculesOwned;
    public Dictionary<MoleculeType, int> moleculeExpertise = new Dictionary<MoleculeType, int>();

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
        moleculesOwned = new Dictionary<MoleculeType, int>();
        moleculesOwned[MoleculeType.A] = StorageA;
        moleculesOwned[MoleculeType.B] = StorageB;
        moleculesOwned[MoleculeType.C] = StorageC;
        moleculesOwned[MoleculeType.D] = StorageD;
        moleculesOwned[MoleculeType.E] = StorageE;

        moleculeExpertise[MoleculeType.A] = ExpertiseA;
        moleculeExpertise[MoleculeType.B] = ExpertiseB;
        moleculeExpertise[MoleculeType.C] = ExpertiseC;
        moleculeExpertise[MoleculeType.D] = ExpertiseD;
        moleculeExpertise[MoleculeType.E] = ExpertiseE;

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
    public Dictionary<MoleculeType, int> MoleculesNeeded = new Dictionary<MoleculeType, int>();

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
        MoleculesNeeded[MoleculeType.A] = CostA;
        MoleculesNeeded[MoleculeType.B] = CostB;
        MoleculesNeeded[MoleculeType.C] = CostC;
        MoleculesNeeded[MoleculeType.D] = CostD;
        MoleculesNeeded[MoleculeType.E] = CostE;

        Console.Error.WriteLine("Sample: "+SampleId+" Total Cost is "+Cost.Sum()+" Health is "+Health);
    }

    public override string ToString()
    {
        return Cost.Aggregate("", (s, agg) => s + " " + agg);
//        return Cost.ToList().ForEach(s => Console.Error.Write(s+" "));
    }
}

public class ScienceProject
{
    public Dictionary<MoleculeType, int> MoleculeExpertiseTargets { get; set; }

    public ScienceProject(int a, int b, int c, int d, int e)
    {

        this.MoleculeExpertiseTargets = new Dictionary<MoleculeType, int>();
        MoleculeExpertiseTargets[MoleculeType.A] = a;
        MoleculeExpertiseTargets[MoleculeType.B] = b;
        MoleculeExpertiseTargets[MoleculeType.C] = c;
        MoleculeExpertiseTargets[MoleculeType.D] = d;
        MoleculeExpertiseTargets[MoleculeType.E] = e;
    }
}

public enum Modules
{
    LABORATORY = 0,
    MOLECULES,
    DIAGNOSIS,
    SAMPLES
}
