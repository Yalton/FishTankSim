using Godot;

public partial class Food : Node3D
{
    public bool IsEaten = false;

    public override void _Ready()
    {
        // Create visual - simple sphere
        var mesh = new MeshInstance3D();
        var sphere = new SphereMesh();
        sphere.Radius = 0.5f;
        sphere.Height = 1f;
        mesh.Mesh = sphere;

        var material = new StandardMaterial3D();
        material.AlbedoColor = new Color(0, 1, 0); // Green
        mesh.MaterialOverride = material;

        AddChild(mesh);
    }

    public void Eat()
    {
        IsEaten = true;
        QueueFree();
    }
}
