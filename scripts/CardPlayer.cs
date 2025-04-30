using Godot;
using System;

public partial class CardPlayer : Node3D
{
    [Export]
    public MeshInstance3D OutlineMesh { get; set; }

    [Export]
    public Area3D HoverArea { get; set; } // Подключите сюда свой Area3D

    [Signal]
    public delegate void ObjectSelectedEventHandler(CardPlayer selectedObject);
    private bool isSelected = false;

    private const String groupHoverName = "HoverObject"; // Keep if used elsewhere

	private bool is_another = false;

    private CardController cc;
    // Параметры анимации перемещения
    [Export]
    public float MoveDuration = 0.3f; // Длительность перемещения в секундах
    [Export]
    public Curve MoveEaseCurve; // Кривая изменения скорости (например, плавное начало и конец)

	public void SetIsAnotherTeam(){
		is_another = true;
	}
    
    public async void MoveTo(CardPlace cp){
        if (cp == null)
        {
            GD.PrintErr("CardPlace is null, cannot move.");
            return;
        }

        //Сохраняем начальную позицию
        Vector3 startPosition = GlobalPosition;
        Vector3 targetPosition = cp.GlobalPosition;

        // Используем Tween для плавного перемещения
        Tween tween = CreateTween();
		if(MoveEaseCurve != null){
			tween.SetEase(Tween.EaseType.Out);
			tween.SetTrans(Tween.TransitionType.Quad);
		}
        
		tween.TweenProperty(this, "global_position", targetPosition, MoveDuration);
    }   
     
    public override void _Ready()
    {
        cc = GetNode<CardController>("/root/CardController");
        if (cc == null)
        {
            GD.PrintErr("CardController не загружен...");
        }
        if (OutlineMesh == null)
        {
            //GD.PrintErr($"OutlineMesh is not assigned in {Name}. Disabling script.");
            SetProcess(false);
            SetPhysicsProcess(false);
            SetProcessInput(false);
            SetProcessUnhandledInput(false);
            return;
        }
        if (HoverArea == null)
        {
            //GD.PrintErr($"HoverArea is not assigned in {Name}. Disabling script.");
            SetProcess(false);
            SetPhysicsProcess(false);
            SetProcessInput(false);
            SetProcessUnhandledInput(false);
            return;
        }

        OutlineMesh.Visible = false;
        HoverArea.InputRayPickable = true; // Ensure raycasts hit this area

        // Connect signals
        HoverArea.MouseEntered += OnAreaHoverEntered;
        HoverArea.MouseExited += OnAreaHoverExited;
        HoverArea.InputEvent += OnAreaInputEvent; // Connect the input event signal

        AddToGroup(groupHoverName); // Keep if used elsewhere
    }
    private void OnAreaHoverEntered()
    {
        if (!isSelected && (!is_another && cc.IsPlayer1Round() || is_another && !cc.IsPlayer1Round())) 
        {
             OutlineMesh.Visible = true;
        }
    }
    private void OnAreaHoverExited()
    {
        // GD.Print($"exited {Name} Area"); // Debug print, can be commented out
        if (!isSelected && (!is_another && cc.IsPlayer1Round() || is_another && !cc.IsPlayer1Round())) // Hide hover effect if not selected
        {
            OutlineMesh.Visible = false;
        }
    }
    private void OnAreaInputEvent(Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeIdx)
    {
        if (@event is InputEventMouseButton mouseButtonEvent)
        {
            GD.Print($"another:{is_another}, player1:{cc.IsPlayer1Round()}");
            // Left Click for Selection
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.Pressed && (!is_another && cc.IsPlayer1Round() || is_another && !cc.IsPlayer1Round()))
            {
                // Only allow selection if the object is currently highlighted (hovered or already selected)
                // This prevents selecting if the mouse clicks *before* the MouseEntered signal fires
                if (OutlineMesh.Visible)
                {
                    SelectObject();
                    // Consume the event to prevent it propagating further (e.g., clicking through the object)
                    GetViewport().SetInputAsHandled();
                }
            }
            // Right Click for Deselection
            else if (mouseButtonEvent.ButtonIndex == MouseButton.Right && mouseButtonEvent.Pressed)
            {
                
                    DeselectObject();
                 
            }
        }
    }


    public void SelectObject()
    {
        // Optional: Add a check to prevent re-selecting if already selected
        // if (isSelected) return;
		Godot.Collections.Array<Node> nnn = GetTree().GetNodesInGroup(groupHoverName);

		foreach(Node n in nnn){
			CardPlayer nn = (CardPlayer)n;
			nn.DeselectObject();
		}
        isSelected = true;
        OutlineMesh.Visible = true; // Ensure outline is visible when selected
        EmitSignal(SignalName.ObjectSelected, this);
        //GD.Print(Name, " selected."); // Changed from "permanently selected" for clarity
    }

    public void DeselectObject()
    {
        isSelected = false;
        OutlineMesh.Visible = false;
        //GD.Print(Name, " deselected.");
    }


    public override void _ExitTree()
    {
        if (HoverArea != null)
        {
            HoverArea.MouseEntered -= OnAreaHoverEntered;
            HoverArea.MouseExited -= OnAreaHoverExited;
            HoverArea.InputEvent -= OnAreaInputEvent;
        }
    }
}
