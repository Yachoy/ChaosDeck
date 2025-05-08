using CCSpace;
using Godot;
using System;

public partial class CardPlace : Node3D
{

	[Export]
    public Area3D HoverArea { get; set; }
	[Signal]
    public delegate void ObjectSelectedEventHandler(CardPlace selectedObject);

	private CardController cc;

    public bool is_another = false;
	public bool is_selected = false;
    public bool isAlreadyCardHerePumPumPum = false;

    public int Index{get; private set;}

	public void SetIsAnotherTeam(){
		is_another = true;
	}
    public void SetIndexed(int index) => Index = index;
    
	
	public override void _Ready()
	{
		cc = GetNode<CardController>("/root/CardController");
        if (cc == null)
        {
            GD.PrintErr("CardController не загружен...");
        }

		HoverArea.InputRayPickable = true; 
		HoverArea.MouseEntered += OnAreaHoverEntered;
		HoverArea.MouseExited += OnAreaHoverExited;
		HoverArea.InputEvent += OnAreaInputEvent;
        HoverArea.AreaEntered += OnAreaEntered;
        HoverArea.AreaExited += OnAreaExit;
	}

    private void OnAreaEntered(Node3D obj) => isAlreadyCardHerePumPumPum = true;
    private void OnAreaExit(Node3D obj) => isAlreadyCardHerePumPumPum = false;


	private void OnAreaHoverEntered() => is_selected = true;

	private void OnAreaHoverExited() => is_selected = false;

    private void OnAreaInputEvent(Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeIdx)
    {
        if (@event is InputEventMouseButton mouseButtonEvent)
        {
			GD.Print($"Click, clicked:{is_selected}, another:{is_another}");
            // Left Click for Selection
            if (mouseButtonEvent.ButtonIndex == MouseButton.Left && mouseButtonEvent.Pressed && ((!is_another && cc.IsPlayer1Round()) || (is_another && !cc.IsPlayer1Round())) )
            {
                if (is_selected){
                    EmitSignal(SignalName.ObjectSelected, this);
                }
            }
        }
    }

}
