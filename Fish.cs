using Godot;
using System.Linq;

public partial class Fish : Node3D
{
    public FishGenes Genes;
    public int FoodEaten = 0;
    private float _currentSpeed = 0;
    private MeshInstance3D _mesh;

    public Fish() { }

    public Fish(FishGenes genes)
    {
        Genes = genes;
    }

    public override void _Ready()
    {
        // Create visual - cone pointing forward
        _mesh = new MeshInstance3D();
        var cone = new CylinderMesh();
        cone.TopRadius = 0;
        cone.BottomRadius = 0.3f;
        cone.Height = 1f;
        _mesh.Mesh = cone;
        _mesh.RotateX(Mathf.Pi / 2); // Point along Z axis

        var material = new StandardMaterial3D();
        material.AlbedoColor = new Color(0.5f, 0.5f, 1f); // Blue
        _mesh.MaterialOverride = material;

        AddChild(_mesh);
    }

    public override void _Process(double delta)
    {
        var tank = GetParent() as Tank;
        if (tank == null) return;

        // Find closest food in vision range
        Food closestFood = null;
        float closestDist = float.MaxValue;

        foreach (var food in tank.GetChildren().OfType<Food>())
        {
            if (food.IsEaten) continue;

            float dist = Position.DistanceTo(food.Position);
            if (dist < Genes.VisionRange && dist < closestDist)
            {
                closestFood = food;
                closestDist = dist;
            }
        }

        // Turn toward food if found
        if (closestFood != null)
        {
            var toFood = (closestFood.Position - Position).Normalized();
            var targetAngle = Mathf.Atan2(toFood.X, toFood.Z);

            // Smoothly rotate toward target
            var currentRotation = Rotation;
            currentRotation.Y = Mathf.LerpAngle(currentRotation.Y, targetAngle,
                Genes.TurnSpeed * (float)delta);
            Rotation = currentRotation;

            // Check if reached food
            if (closestDist < 1f)
            {
                closestFood.Eat();
                FoodEaten++;
            }
        }

        // Accelerate to max speed
        _currentSpeed = Mathf.MoveToward(_currentSpeed, Genes.MaxSpeed, 5f * (float)delta);

        // Move forward
        var forward = -Transform.Basis.Z;
        Position += forward * _currentSpeed * (float)delta;

        // Wrap around tank boundaries
        var pos = Position;
        if (pos.X < -25) pos.X = 25;
        if (pos.X > 25) pos.X = -25;
        if (pos.Z < -25) pos.Z = 25;
        if (pos.Z > 25) pos.Z = -25;
        Position = pos;
    }
}
