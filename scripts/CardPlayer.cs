using Godot;
using System;

public partial class CardPlayer : Node3D
{
    [Export]
    public MeshInstance3D OutlineMesh { get; set; }
    [Export]
    public Area3D HoverArea { get; set; }
    [Export]
    public Sprite3D imgCard {get;set;}
    [Export]
    public Godot.Collections.Array<Node> nodes;
    [Export]
    public bool isCardHidden {get; private set;}
    [Export]
    private bool isSelected = false;
	[Export]
    private bool is_another = false;
    [Export]
    public bool is_placed = false;

    [Signal]
    public delegate void ObjectSelectedEventHandler(CardPlayer selectedObject);
    

    private const String groupHoverName = "HoverObject"; // Keep if used elsewhere

    private CardController cc;
    // Параметры анимации перемещения
    [Export]
    public float MoveDuration = 0.3f; // Длительность перемещения в секундах
    [Export]
    public Curve MoveEaseCurve; // Кривая изменения скорости (например, плавное начало и конец)

    private Texture2D _previousTexture;

	public void SetIsAnotherTeam(){
		is_another = true;
	}

    public CCSpace.CardData.Card cardInstanceInfo;
	public void GenerateCard(){
        cardInstanceInfo = cc.GetNewCard(is_another);
        ChangeTexture(cardInstanceInfo.ImageTexture);
        UpdateStatisticeOnTheCards();
    }
    
    public void UpdateStatisticeOnTheCards(){
        nodes[1].GetChild<Label3D>(0).Text = cardInstanceInfo.Cost.ToString();
        nodes[2].GetChild<Label3D>(0).Text = cardInstanceInfo.Hp.ToString();
        nodes[3].GetChild<Label3D>(0).Text = cardInstanceInfo.Damage.ToString();
    }

    public bool SwitchViewCard(){
        if (is_placed) return false;
        foreach(Node3D n in nodes){
            n.Visible = !n.Visible;
            isCardHidden = n.Visible;
        }
        Vector3 scl = imgCard.Scale;
        Vector3 changeS = new Vector3(0.01f, 0.04f, 0);
        if (isCardHidden){
            scl -= changeS;
            ResetTexture();
        }else{
            scl += changeS;
            //TODO: ЗАГЛУШКА! Need to change to correct back side card
            ChangeTexture("res://resources/images/backCard.png");
        }
        imgCard.Scale = scl;
        return true;
    }

	public void ChangeTexture(string newTexturePath)
	{
		_previousTexture = imgCard.Texture;
		var newTexture = GD.Load<Texture2D>(newTexturePath);
        ChangeTexture(newTexture);
	}

    public void ChangeTexture(Texture2D texture)
	{
		_previousTexture = imgCard.Texture;
		
		if (texture != null)
		{
			imgCard.Texture = texture;
		}
		else
		{
			GD.PrintErr($"Error: Could not load texture");
			if (_previousTexture != null)
			{
				imgCard.Texture = _previousTexture;
			}
			_previousTexture = null; // Сбрасываем предыдущую, так как текущая не изменилась
		}

        AdjustSpriteToTargetObject();
	}

    public void ResetTexture(){
        imgCard.Texture = _previousTexture;
        _previousTexture = null;
        AdjustSpriteToTargetObject();
    }

    // Основная функция для подстройки размера спрайта под объект
	public void AdjustSpriteToTargetObject()
	{

        if (cc.GetCurrentPlayer().mana >= cardInstanceInfo.Cost){
            imgCard.Modulate = new Color(1, 1, 1, 1);
            // GD.Print("NIIIGAB");
        }else{
            // GD.Print("No nigab ", $"{cc.GetCurrentPlayer().mana}, {cardInstanceInfo.Cost}");
            imgCard.Modulate = new Color(0.6f, 0.6f, 0.6f, 1);
        }
		
        Vector3 objectSize = Scale * 5.8f; // Или используйте AABB.Size

		// Применяем отступ к размерам объекта
		float effectiveWidth = objectSize.X * (1.0f - 2.0f * 0.1f);
		float effectiveHeight = objectSize.Y * (1.0f - 2.0f * 0.1f); // Предполагаем, что Y - это высота в 3D

		// Получаем размеры текстуры в пикселях
		Vector2 textureSize = imgCard.Texture.GetSize();

		// Избегаем деления на ноль
		if (textureSize.X <= 0 || textureSize.Y <= 0)
		{
			GD.PrintErr("Texture has invalid dimensions.");
			return;
		}

		// Рассчитываем пропорции текстуры
		float textureAspectRatio = textureSize.X / textureSize.Y;

		// Рассчитываем пропорции эффективной области объекта
		// В Godot 3D, Sprite3D по умолчанию направлен так, что Y - это высота.
		// Поэтому эффективная высота соответствует Y объекта, а эффективная ширина - X.
		float effectiveAspectRatio = effectiveWidth / effectiveHeight;

		// Определяем, по какой стороне мы будем масштабировать
		float targetPixelSize;
		if (textureAspectRatio > effectiveAspectRatio)
		{
			// Текстура шире относительно своей высоты, чем эффективная область.
			// Ограничиваемся эффективной шириной объекта.
			// EffectiveWidth = TextureWidth * PixelSize
			// PixelSize = EffectiveWidth / TextureWidth
			targetPixelSize = effectiveWidth / textureSize.X;
		}
		else
		{
			// Текстура выше относительно своей ширины, чем эффективная область.
			// Ограничиваемся эффективной высотой объекта.
			// EffectiveHeight = TextureHeight * PixelSize
			// PixelSize = EffectiveHeight / TextureHeight
			targetPixelSize = effectiveHeight / textureSize.Y;
		}

		imgCard.PixelSize = targetPixelSize;
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
        isCardHidden = false;
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
