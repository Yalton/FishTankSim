using System;

public class FishGenes
{
    public float VisionRange;  // How far fish can see food
    public float TurnSpeed;    // Radians per second
    public float MaxSpeed;     // Units per second

    public FishGenes(float vision, float turn, float speed)
    {
        VisionRange = vision;
        TurnSpeed = turn;
        MaxSpeed = speed;
    }

    public FishGenes Mutate()
    {
        // Add ±10% random variation to each gene
        var rng = new Random();
        return new FishGenes(
            VisionRange * (1 + (float)(rng.NextDouble() - 0.5) * 0.2f),
            TurnSpeed * (1 + (float)(rng.NextDouble() - 0.5) * 0.2f),
            MaxSpeed * (1 + (float)(rng.NextDouble() - 0.5) * 0.2f)
        );
    }

    public static FishGenes Random()
    {
        var rng = new Random();
        return new FishGenes(
            (float)(rng.NextDouble() * 20 + 15),  // 15-35 units (better starting vision)
            (float)(rng.NextDouble() * 2 + 2),    // 2-4 rad/s (faster turning)
            (float)(rng.NextDouble() * 4 + 4)     // 4-8 units/s
        );
    }
}
