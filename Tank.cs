using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class Tank : Node3D
{
    private const int PopulationSize = 30;
    private const float GenerationTime = 45f; // seconds
    private const float FoodSpawnInterval = 1.5f; // seconds
    private const int InitialFoodCount = 10; // food at start of each generation

    private List<Fish> _currentGeneration = new List<Fish>();
    private int _generationNumber = 0;
    private float _generationTimer = 0;
    private float _foodTimer = 0;
    private Label _statsLabel;

    public override void _Ready()
    {
        // Find stats label
        _statsLabel = GetNode<Label>("../UI/StatsLabel");

        // Start first generation
        SpawnGeneration(null);
    }

    public override void _Process(double delta)
    {
        _generationTimer += (float)delta;
        _foodTimer += (float)delta;

        // Spawn food periodically
        if (_foodTimer >= FoodSpawnInterval)
        {
            _foodTimer = 0;
            SpawnFood();
        }

        // End generation after time limit
        if (_generationTimer >= GenerationTime)
        {
            EndGeneration();
        }

        // Update UI
        UpdateStats();
    }

    private void SpawnGeneration(List<FishGenes> parentGenes)
    {
        _generationNumber++;
        _generationTimer = 0;
        _currentGeneration.Clear();

        // Clear old fish and food
        foreach (var child in GetChildren())
        {
            child.QueueFree();
        }

        // Generate new fish
        for (int i = 0; i < PopulationSize; i++)
        {
            FishGenes genes;
            if (parentGenes == null || parentGenes.Count == 0)
            {
                // Random genes for first generation
                genes = FishGenes.Random();
            }
            else
            {
                // Pick random parent and mutate
                genes = parentGenes[GD.RandRange(0, parentGenes.Count - 1)].Mutate();
            }

            var fish = new Fish(genes);
            fish.Position = new Vector3(
                (float)GD.RandRange(-20, 20),
                0,
                (float)GD.RandRange(-20, 20)
            );
            fish.Rotation = new Vector3(0, (float)GD.RandRange(0, Mathf.Tau), 0);

            AddChild(fish);
            _currentGeneration.Add(fish);
        }

        // Spawn initial food
        for (int i = 0; i < InitialFoodCount; i++)
        {
            SpawnFood();
        }
    }

    private void SpawnFood()
    {
        var food = new Food();
        food.Position = new Vector3(
            (float)GD.RandRange(-20, 20),
            0,
            (float)GD.RandRange(-20, 20)
        );
        AddChild(food);
    }

    private void EndGeneration()
    {
        // Sort by fitness
        var sorted = _currentGeneration.OrderByDescending(f => f.FoodEaten).ToList();

        // Take top 50% as parents
        int parentCount = PopulationSize / 2;
        var parents = sorted.Take(parentCount).Select(f => f.Genes).ToList();

        // Start new generation
        SpawnGeneration(parents);
    }

    private void UpdateStats()
    {
        if (_statsLabel == null) return;

        int totalFood = _currentGeneration.Sum(f => f.FoodEaten);
        float avgFood = _currentGeneration.Count > 0 ?
            (float)totalFood / _currentGeneration.Count : 0;

        _statsLabel.Text = $"Generation: {_generationNumber}\n" +
                          $"Time: {_generationTimer:F1}s / {GenerationTime}s\n" +
                          $"Avg Fitness: {avgFood:F2}";
    }
}
