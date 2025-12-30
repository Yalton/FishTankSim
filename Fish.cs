using Godot;
using System.Linq;

public partial class Fish : Node3D
{
    public FishGenes Genes;
    public int FoodEaten = 0;
    private float _currentSpeed = 0;
    private MeshInstance3D _mesh;
    private float _wanderAngle = 0;
    private float _wanderTimer = 0;
    private const float TankBounds = 24f;

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

        float targetAngle = Rotation.Y;

        // Check if near edges - turn toward center
        bool nearEdge = false;
        var pos = Position;
        if (Mathf.Abs(pos.X) > TankBounds || Mathf.Abs(pos.Z) > TankBounds)
        {
            nearEdge = true;
            var toCenter = -Position.Normalized();
            targetAngle = Mathf.Atan2(toCenter.X, toCenter.Z);
        }
        // Turn toward food if found
        else if (closestFood != null)
        {
            var toFood = (closestFood.Position - Position).Normalized();
            targetAngle = Mathf.Atan2(toFood.X, toFood.Z);

            // Check if reached food
            if (closestDist < 1f)
            {
                closestFood.Eat();
                FoodEaten++;
            }
        }
        // Wander randomly when no food visible
        else
        {
            _wanderTimer -= (float)delta;
            if (_wanderTimer <= 0)
            {
                _wanderTimer = (float)GD.RandRange(0.5, 2.0);
                _wanderAngle = Rotation.Y + (float)GD.RandRange(-1.0, 1.0);
            }
            targetAngle = _wanderAngle;
        }

        // Smoothly rotate toward target
        var currentRotation = Rotation;
        float turnRate = nearEdge ? Genes.TurnSpeed * 3f : Genes.TurnSpeed;
        currentRotation.Y = Mathf.LerpAngle(currentRotation.Y, targetAngle,
            turnRate * (float)delta);
        Rotation = currentRotation;

        // Accelerate to max speed
        _currentSpeed = Mathf.MoveToward(_currentSpeed, Genes.MaxSpeed, 5f * (float)delta);

        // Move forward
        var forward = -Transform.Basis.Z;
        Position += forward * _currentSpeed * (float)delta;

        // Hard clamp to tank boundaries (safety net)
        pos = Position;
        pos.X = Mathf.Clamp(pos.X, -TankBounds - 1, TankBounds + 1);
        pos.Z = Mathf.Clamp(pos.Z, -TankBounds - 1, TankBounds + 1);
        Position = pos;
    }
}
