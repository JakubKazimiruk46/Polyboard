using Godot;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Polyboard.Enums;


public partial class Board : StaticBody3D
{
	private Vector3 targetPosition;
	private List<Field> fields = new List<Field>();
	private Sprite2D textureDisplay;
	private Sprite2D randomCard;
	private Label ownerNickname;
	private Label mortgageInfo;
	private Sprite2D step_on_card;
	private Button endTurnButton;
	private TextureButton tradeButton;
	private TextureButton buildButton;
	private GameManager gameManager;
	private CanvasLayer BuyCard;
	private TextureRect cardView;
	private Timer buyTime;
	private Field field;
	private PanelContainer ownerNicknameView;
	protected AudioStreamPlayer3D deanOfficeSoundPlayer;
	protected AudioStreamPlayer3D lostECTSSoundPlayer;
	private PopupMenu _contextMenu;
	private Vector2 _lastMousePosition;
	private Label _popupInfoLabel;
	private Node achievementManager;
	private enum PopupIds
	{
		SellProperty = 100,
		SellHouse = 101,
		BuildHouse = 102,
		ExchangeProperty = 103,
		Cancel = 104,
		UseSpecialCard = 105,
		SetOnMortgage = 106,
		CancelMortgage = 107
	}

	private readonly Dictionary<(string type, int number), (int ectsEffect, Func<Task> specialEffect)> cardEffects;

	public Board()
	{
		cardEffects = new Dictionary<(string type, int number), (int ectsEffect, Func<Task> specialEffect)>();
	}

	public override void _Ready()
	{
		InitializeComponents();
		InitializeCardEffects();
		InitializePopupMenu();
		InitAchievementManager();
	}
	private void InitAchievementManager()
	{
		achievementManager = GetNodeOrNull<Node>("/root/Level/GameManager/AchievementManager");
		if (achievementManager == null)
		{
			GD.Print("Nie znaleziono AchievementManagera.");
		}
	}
	private void InitializeComponents()
	{
		step_on_card = GetNodeOrNull<Sprite2D>("/root/Level/BuyCard/HBoxContainer/FieldView/TextureRect/FieldToBuy");
		randomCard = GetNodeOrNull<Sprite2D>("/root/Level/CanvasLayer/TextureRect2/RandomCard");
		textureDisplay = GetNodeOrNull<Sprite2D>("/root/Level/CanvasLayer/TextureRect2/FieldCard");
		ownerNickname = GetNodeOrNull<Label>("/root/Level/CanvasLayer/OwnerNickname/VBoxContainer/owner_nickname");
		mortgageInfo = GetNodeOrNull<Label>("/root/Level/CanvasLayer/OwnerNickname/VBoxContainer/mortgage_info");
		cardView = GetNodeOrNull<TextureRect>("/root/Level/BuyCard/HBoxContainer/FieldView/TextureRect");
		buyTime = GetNodeOrNull<Timer>("/root/Level/BuyCard/Timer");
		endTurnButton = GetNodeOrNull<Button>("/root/Level/UI/ZakończTure");
		tradeButton = GetNodeOrNull<TextureButton>("/root/Level/UI/HBoxContainer/PanelContainer/MarginContainer/Buttons/VBoxContainer2/trade_button");
		buildButton = GetNodeOrNull<TextureButton>("/root/Level/UI/HBoxContainer/PanelContainer/MarginContainer/Buttons/VBoxContainer3/build_button");
		gameManager = GetNode<GameManager>("/root/Level/GameManager");
		ownerNicknameView = GetNodeOrNull<PanelContainer>("/root/Level/CanvasLayer/OwnerNickname");
		deanOfficeSoundPlayer = GetNodeOrNull<AudioStreamPlayer3D>("/root/Level/Board/DeanOfficeSound");
		lostECTSSoundPlayer = GetNodeOrNull<AudioStreamPlayer3D>("/root/Level/Board/LostECTSSound");
		
		if (textureDisplay == null || step_on_card == null)
		{
			GD.PrintErr("Błąd: Nie znaleziono wymaganych komponentów Sprite2D.");
			return;
		}

		targetPosition = GlobalPosition;
		BuyCard = GetTree().Root.GetNode<CanvasLayer>("Level/BuyCard");
		foreach (Node child in GetChildren())
		{
			if (child is Field field)
			{
				switch (field.FieldId)
				{
					case 1:
						field.Name = "Katedra Leśnictwa";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 60;
						field.Department = DepartmentName.Lesnictwa;
						field.rentCost = new List<int> {2, 10, 30, 90, 160, 250};
						break;
					case 3:
						field.Name = "Katedra Drzewek";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 60;
						field.Department = DepartmentName.Lesnictwa;
						field.rentCost = new List<int> {2, 20, 60, 180, 320, 450};
						break;
					case 5:
						field.Name = "Akademik Alfa";
						field.fieldCost = 200;
						field.rentCost = new List<int> {25, 50, 100, 200};
						break;
					case 6:
						field.Name = "Katedra Ekonomii";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 100;
						field.Department = DepartmentName.Zarzadzania;
						field.rentCost = new List<int> {6, 30, 90, 270, 400, 550};
						break;
					case 8:
						field.Name = "Katedra Marketingu";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 100;
						field.Department = DepartmentName.Zarzadzania;
						field.rentCost = new List<int> {6, 30, 90, 270, 400, 550};
						break;
					case 9:
						field.Name = "Katedra Logistyki";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 120;
						field.Department = DepartmentName.Zarzadzania;
						field.rentCost = new List<int> {8, 40, 100, 300, 450, 600};
						break;
					case 11:
						field.Name = "Katedra Grafiki";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 140;
						field.Department = DepartmentName.Architektury;
						field.rentCost = new List<int> {10, 50, 150, 450, 625, 750};
						break;
					case 12:
						field.Name = "Centrum Nowoczesnego Kształcenia";
						field.fieldCost = 150;
						break;
					case 13:
						field.Name = "Katedra Urbanistyki";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 140;
						field.Department = DepartmentName.Architektury;
						field.rentCost = new List<int> {10, 50, 150, 450, 625, 750};
						break;
					case 14:
						field.Name = "Katedra Architektury Wnętrz";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 160;
						field.Department = DepartmentName.Architektury;
						field.rentCost = new List<int> {10, 50, 150, 450, 625, 750};
						break;
					case 15:
						field.Name = "Akademik Beta";
						field.fieldCost = 200;
						field.rentCost = new List<int> {25, 50, 100, 200};
						break;
					case 16:
						field.Name = "Katedra Geotechniki";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 180;
						field.Department = DepartmentName.Budownictwa;
						field.rentCost = new List<int> {14, 70, 200, 550, 750, 950};
						break;
					case 18:
						field.Name = "Katedra Budownictwa";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 180;
						field.Department = DepartmentName.Budownictwa;
						field.rentCost = new List<int> {14, 70, 200, 550, 750, 950};
						break;
					case 19:
						field.Name = "Katedra Konstrukcji Budowlanych";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 200;
						field.Department = DepartmentName.Budownictwa;
						field.rentCost = new List<int> {16, 80, 220, 600, 800, 1000};
						break;
					case 21:
						field.Name = "Katedra Biotechnologii";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 220;
						field.Department = DepartmentName.InzynieriiSrodowiska;
						field.rentCost = new List<int> {18, 90, 250, 700, 875, 1050};
						break;
					case 23:
						field.Name = "Katedra Ciepłownictwa";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 220;
						field.Department = DepartmentName.InzynieriiSrodowiska;
						field.rentCost = new List<int> {18, 90, 250, 700, 875, 1050};
						break;
					case 24:
						field.Name = "Katedra Inżynierii Rolnej";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 240;
						field.Department = DepartmentName.InzynieriiSrodowiska;
						field.rentCost = new List<int> {20, 100, 300, 750, 925, 1110};
						break;
					case 25:
						field.Name = "Akademik Gamma";
						field.fieldCost = 200;
						field.rentCost = new List<int> {25, 50, 100, 200};
						break;
					case 26:
						field.Name = "Katedra Mechaniki";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 260;
						field.Department = DepartmentName.Mechaniczny;
						field.rentCost = new List<int> {22, 110, 330, 800, 975, 1150};
						break;
					case 27:
						field.Name = "Katedra Budowy Maszyn";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 260;
						field.Department = DepartmentName.Mechaniczny;
						field.rentCost = new List<int> {22, 110, 330, 800, 975, 1150};
						break;
					case 28:
						field.Name = "Klub Gwint";
						field.fieldCost = 150;
						break;
					case 29:
						field.Name = "Katedra Inżynierii Biomedycznej";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 280;
						field.Department = DepartmentName.Mechaniczny;
						field.rentCost = new List<int> {24, 120, 360, 850, 1025, 1200};
						break;
					case 31:
						field.Name = "Katedra Automatyki i Robotyki";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 300;
						field.Department = DepartmentName.Elektryczny;
						field.rentCost = new List<int> {26, 130, 390, 900, 1100, 1275};
						break;
					case 32:
						field.Name = "Katedra Elektrotechniki";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 300;
						field.Department = DepartmentName.Elektryczny;
						field.rentCost = new List<int> {26, 130, 390, 900, 1100, 1275};
						break;
					case 34:
						field.Name = "Katedra Fotoniki";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 320;
						field.Department = DepartmentName.Elektryczny;
						field.rentCost = new List<int> {28, 150, 450, 1000, 1200, 1400};
						break;
					case 35:
						field.Name = "Akademik Delta";
						field.fieldCost = 200;
						field.rentCost = new List<int> {25, 50, 100, 200};
						break;
					case 37:
						field.Name = "Katedra Oprogramowania";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 350;
						field.Department = DepartmentName.Informatyczny;
						field.rentCost = new List<int> {35, 175, 500, 1100, 1300, 1500};
						break;
					case 39:
						field.Name = "Katedra Matematyki";
						field.houseCost = 150;
						field.hotelCost = 150;
						field.fieldCost = 400;
						field.Department = DepartmentName.Informatyczny;
						field.rentCost = new List<int> {50, 200, 600, 1400, 1700, 2000};
						break;
				}
				fields.Add(field);
			}
		}
	}
private void InitializePopupMenu()
{
	_contextMenu = new PopupMenu();
	AddChild(_contextMenu);
	
	var theme = new Theme();

	var panelStyle = new StyleBoxFlat();
	panelStyle.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);
	panelStyle.BorderWidthBottom = 3;
	panelStyle.BorderWidthLeft = 3;
	panelStyle.BorderWidthRight = 3;
	panelStyle.BorderWidthTop = 3;
	panelStyle.BorderColor = new Color("#62ff45"); 
	panelStyle.CornerRadiusBottomLeft = 8;
	panelStyle.CornerRadiusBottomRight = 8;
	panelStyle.CornerRadiusTopLeft = 8;
	panelStyle.CornerRadiusTopRight = 8;
	panelStyle.ContentMarginLeft = 10;
	panelStyle.ContentMarginRight = 10;
	panelStyle.ContentMarginTop = 5;
	panelStyle.ContentMarginBottom = 5;
	
	var hoverStyle = new StyleBoxFlat();
	hoverStyle.BgColor = new Color(0.3f, 0.3f, 0.3f); 
	hoverStyle.CornerRadiusBottomLeft = 8;
	hoverStyle.CornerRadiusBottomRight = 8;
	hoverStyle.CornerRadiusTopLeft = 8;
	hoverStyle.CornerRadiusTopRight = 8;
	
	theme.SetStylebox("panel", "PopupMenu", panelStyle);
	theme.SetStylebox("hover", "PopupMenu", hoverStyle);
	theme.SetColor("font_color", "PopupMenu", new Color(1, 1, 1)); 
	theme.SetColor("font_hover_color", "PopupMenu", new Color("#62ff45")); 
	
	_contextMenu.Theme = theme;
	
	_contextMenu.AddItem("Sell Property", (int)PopupIds.SellProperty);
	_contextMenu.AddItem("Sell House", (int)PopupIds.SellHouse);
	_contextMenu.AddItem("Build House", (int)PopupIds.BuildHouse);
	_contextMenu.AddItem("Exchange Property", (int)PopupIds.ExchangeProperty);
	_contextMenu.AddItem("Use Special Card", (int)PopupIds.UseSpecialCard);
	_contextMenu.AddItem("Set property on mortgage", (int)PopupIds.SetOnMortgage);
	_contextMenu.AddItem("Cancel mortgage", (int)PopupIds.CancelMortgage);
	
	_contextMenu.AddSeparator();
	_contextMenu.AddItem("Cancel", (int)PopupIds.Cancel);
	
	_contextMenu.IdPressed += OnPopupMenuItemPressed;

}
	
	private void OnPopupMenuItemPressed(long id)
	{
		var camera = GetViewport().GetCamera3D();
		var from = camera.ProjectRayOrigin(_lastMousePosition);
		var to = from + camera.ProjectRayNormal(_lastMousePosition) * 100;
		
		var spaceState = GetWorld3D().DirectSpaceState;
		var query = PhysicsRayQueryParameters3D.Create(from, to);
		var result = spaceState.IntersectRay(query);
		
		Field selectedField = null;
		
		if (result.Count > 0)
		{
			var collider = result["collider"].AsGodotObject();
			if (collider is Field field)
			{
				selectedField = field;
			}
			else
			{
				Vector3 hitPosition = (Vector3)result["position"];
				selectedField = FindNearestField(hitPosition);
			}
		}
		
		Figurehead currentPlayer = gameManager.getCurrentPlayer();
		
	if (selectedField != null && currentPlayer != null)
	{
		switch ((PopupIds)id)
		{
			case PopupIds.SellProperty:
				TrySellProperty(selectedField, currentPlayer);
				break;
				
			case PopupIds.SellHouse:
				TrySellHouse(selectedField, currentPlayer);
				break;
				
			case PopupIds.BuildHouse:
				TryBuildHouse(selectedField, currentPlayer);
				break;
				
			case PopupIds.ExchangeProperty:
				TryExchangeProperty(selectedField, currentPlayer);
				break;
				
			case PopupIds.UseSpecialCard:
				//TryUseSpecialCard(currentPlayer);
				break;
				
			case PopupIds.SetOnMortgage:
				TrySetOnMortgage(selectedField,currentPlayer);
				break;
				
			case PopupIds.CancelMortgage:
				TryCancelMortgage(selectedField,currentPlayer);
				break;
				
			case PopupIds.Cancel:
				break;
		}
	}
		else
		{
			if (selectedField == null)
			{
				ShowPopupNotification("No field selected. Click closer to a field.", 2.0f);
			}
		}
	}
private void TryCancelMortgage(Field field, Figurehead player)
{
	string ownerClassName = field.Owner?.GetType().Name ?? "null";
	bool playerOwnsAccordingToArray = player.ownedFields[field.FieldId];
	if (field.owned && playerOwnsAccordingToArray)
	{
		int cancelValue = field.fieldCost / 2 + field.fieldCost / 4;
		field.CancelMortgage(player,field);
		ShowPopupNotification($"{player.Name} canceled mortgage on {field.Name} and spent {cancelValue} ECTS", 3.0f);
	}
	else
	{
		ShowPopupNotification("You don't own this property!", 2.0f);
		GD.Print($"Ownership check failed: owned={field.owned}, playerOwnsAccordingToArray={playerOwnsAccordingToArray}");
	}
}
private void TrySetOnMortgage(Field field, Figurehead player)
{
	string ownerClassName = field.Owner?.GetType().Name ?? "null";
	bool playerOwnsAccordingToArray = player.ownedFields[field.FieldId];
	if (field.owned && playerOwnsAccordingToArray)
	{
		int mortgageValue = field.fieldCost / 2;
		field.OnMortgage(player,field);
		ShowPopupNotification($"{field.Name} is on mortgage. {player.Name} gain {mortgageValue} ECTS", 3.0f);
	}
	else
	{
		ShowPopupNotification("You don't own this property!", 2.0f);
		GD.Print($"Ownership check failed: owned={field.owned}, playerOwnsAccordingToArray={playerOwnsAccordingToArray}");
	}
}
private void TrySellProperty(Field field, Figurehead player)
{
	int currentPlayerIndex = gameManager.GetCurrentPlayerIndex();
	
	string ownerClassName = field.Owner?.GetType().Name ?? "null";
	
	// GD.Print($"Field: {field.Name}, FieldId: {field.FieldId}");
	// GD.Print($"Field owned: {field.owned}, Owner name: {field.Owner?.Name}");
	// GD.Print($"Owner class name: {ownerClassName}");
	// GD.Print($"Current player name: {player.Name}, Index: {currentPlayerIndex}");

	bool playerOwnsAccordingToArray = player.ownedFields[field.FieldId];
	// GD.Print($"Player owns according to ownedFields array: {playerOwnsAccordingToArray}");
	
	if (field.owned && playerOwnsAccordingToArray)
	{
		// Calculate sell value (half of purchase price)
		int sellValue = field.fieldCost / 2;
		
		// GD.Print($"Selling field: {field.Name} for {sellValue} ECTS");
		
		field.RemoveOwner(player, field);
		
		ShowPopupNotification($"Sold {field.Name} for {sellValue} ECTS", 3.0f);
		
		gameManager.UpdateECTSUI(currentPlayerIndex);
	}
	else
	{
		ShowPopupNotification("You don't own this property!", 2.0f);
		GD.Print($"Ownership check failed: owned={field.owned}, playerOwnsAccordingToArray={playerOwnsAccordingToArray}");
	}
}

private void TrySellHouse(Field field, Figurehead player)
{
	if (field.owned && field.Owner == player)
	{
		int houseCount = field.CheckHouseQuantity(field);
		if (houseCount > 0)
		{
			if (field.isHotel)
			{
				ShowPopupNotification("Cannot sell individual houses from a hotel. Sell the hotel instead.", 3.0f);
				return;
			}
			
			int sellValue = field.houseCost / 2;
			
			RemoveLastHouse(field);
			
			player.AddECTS(sellValue);
			
			ShowPopupNotification($"Sold house on {field.Name} for {sellValue} ECTS", 3.0f);
			
			gameManager.UpdateECTSUI(gameManager.GetCurrentPlayerIndex());
		}
		else
		{
			ShowPopupNotification("No houses to sell on this property!", 2.0f);
		}
	}
	else
	{
		ShowPopupNotification("You don't own this property!", 2.0f);
	}
}

private void RemoveLastHouse(Field field)
{
	int lastIndex = -1;
	for (int i = field.buildOccupied.Count - 1; i >= 0; i--)
	{
		if (field.buildOccupied[i])
		{
			lastIndex = i;
			break;
		}
	}
	
	if (lastIndex >= 0 && lastIndex < field.builtHouses.Count)
	{
		if (field.builtHouses[lastIndex] != null)
		{
			field.builtHouses[lastIndex].QueueFree();
			field.builtHouses.RemoveAt(lastIndex);
		}
		
		// Oznacz pozycję jako niezajętą
		field.buildOccupied[lastIndex] = false;
	}
}

private List<Field> GetAllFieldsByDepartmentName(DepartmentName departmentName){
	List<Field> fields = new List<Field>();
	
	foreach (Node child in GetChildren())
		{
			if (child is Field field && field.Department == departmentName)
			{
				fields.Add(field);
			}
		}
	return fields;
}

private bool DoesPlayerOwnFields(List<Field> fields, Figurehead player){
	foreach (Field field in fields){
		if (!field.owned || field.Owner != player)
			return false;
	}
	
	return true;
}


private void TryBuildHouse(Field field, Figurehead player)
{
	GD.Print("Inside funtcion TryBuildHouse");
	
	if (field.owned && field.Owner == player)
	{	
		DepartmentName departmentName = field.Department;
		
		if (!DoesPlayerOwnFields(GetAllFieldsByDepartmentName(departmentName), player)){
			ShowPopupNotification("You need to own all the fields in department to build!", 2.0f);
			return;
		}
		
		HashSet<int> invalidFieldIds = new HashSet<int> { 0, 2, 4, 5, 7, 10, 12, 15, 17, 20, 22, 25, 28, 30, 33, 35, 36, 38 };
		if (invalidFieldIds.Contains(field.FieldId))
		{
			ShowPopupNotification("Cannot build houses on this type of property!", 2.0f);
			return;
		}
		
		if (player.ECTS >= field.houseCost)
		{
			if (field.isHotel)
			{
				ShowPopupNotification("There's already a hotel on this property!", 2.0f);
				return;
			}
			
			int houseCount = field.CheckHouseQuantity(field);
			if (houseCount < 4)
			{
				player.SpendECTS(field.houseCost);
				
				field.BuildingHouse(field.FieldId);
				achievementManager.Call("track_hotel_built", field.Department.ToString());
				ShowPopupNotification($"Building house on {field.Name} for {field.houseCost} ECTS", 3.0f);

				gameManager.UpdateECTSUI(gameManager.GetCurrentPlayerIndex());
			}
			else
			{
				ShowPopupNotification("Maximum number of houses reached! Consider building a hotel.", 2.0f);
			}
		}
		else
		{
			ShowPopupNotification("Not enough ECTS to build a house!", 2.0f);
		}
	}
	else
	{
		ShowPopupNotification("You don't own this property!", 2.0f);
	}
}

private async void TryBuildHotel(Field field, Figurehead player)
{
	if (field.owned && field.Owner == player)
	{
		HashSet<int> invalidFieldIds = new HashSet<int> { 0, 2, 4, 5, 7, 10, 12, 15, 17, 20, 22, 25, 28, 30, 33, 35, 36, 38 };
		if (invalidFieldIds.Contains(field.FieldId))
		{
			ShowPopupNotification("Cannot build a hotel on this type of property!", 2.0f);
			return;
		}
		
		if (player.ECTS >= field.hotelCost)
		{
			if (field.isHotel)
			{
				ShowPopupNotification("There's already a hotel on this property!", 2.0f);
				return;
			}
			
			int houseCount = field.CheckHouseQuantity(field);
			if (houseCount == 4)
			{
				player.SpendECTS(field.hotelCost);
				
				await field.BuildHotel(field.FieldId);
				achievementManager.Call("track_hotel_build", field.Department.ToString());
				ShowPopupNotification($"Built hotel on {field.Name} for {field.hotelCost} ECTS", 3.0f);
				
				gameManager.UpdateECTSUI(gameManager.GetCurrentPlayerIndex());
			}
			else
			{
				ShowPopupNotification($"You need 4 houses before building a hotel! (Current: {houseCount})", 3.0f);
			}
		}
		else
		{
			ShowPopupNotification("Not enough ECTS to build a hotel!", 2.0f);
		}
	}
	else
	{
		ShowPopupNotification("You don't own this property!", 2.0f);
	}
}

private void TryExchangeProperty(Field field, Figurehead player)
{

	if (field.owned && field.Owner == player)
	{
		// Get the trade UI
		var tradeUI = GetNode<CanvasLayer>("/root/Level/Trade");
		if (tradeUI != null)
		{

			Godot.Collections.Array playerData = new Godot.Collections.Array();
			
			for (int i = 0; i < gameManager.Players.Count; i++)
			{
				var p = gameManager.Players[i];
				var playerInfo = new Godot.Collections.Dictionary
				{
					{ "name", p.Name },
					{ "id", i }
				};
				playerData.Add(playerInfo);
			}
			
			tradeUI.Call("setup_trade", player.Name, playerData, field.FieldId);
			
			ShowPopupNotification("Otwieranie interfejsu wymiany...", 2.0f);
		}
		else
		{
			ShowPopupError("Nie znaleziono interfejsu wymiany!", 2.0f);
		}
	}
	else
	{
		ShowPopupNotification("Nie jesteś właścicielem tego pola!", 2.0f);
	}
}
private void ShowPopupNotification(string message, float duration = 3.0f)
{
	var notifications = GetNode<Node>("/root/Notifications");
	if (notifications != null)
	{
		notifications.Call("show_notification", message, duration);
	}
	else
	{
		GD.PrintErr("NotificationLayer singleton not found. Make sure it's added as an Autoload.");
		
		_popupInfoLabel.Text = message;
		_popupInfoLabel.Visible = true;
		
		GetTree().CreateTimer(duration).Timeout += () => _popupInfoLabel.Visible = false;
	}
}
private void ShowPopupError(string message, float duration = 4.0f)
{
	var notifications = GetNode<Node>("/root/Notifications");
	if (notifications != null)
	{
		notifications.Call("show_error", message, duration);
	}
	else
	{
		GD.PrintErr("NotificationLayer singleton not found. Make sure it's added as an Autoload.");
		GD.PrintErr(message);
		
		_popupInfoLabel.Text = message;
		_popupInfoLabel.Visible = true;
		
		GetTree().CreateTimer(duration).Timeout += () => _popupInfoLabel.Visible = false;
	}
}
	
	private Field FindNearestField(Vector3 position)
	{
		Field nearestField = null;
		float minDistance = float.MaxValue;
		
		foreach (Field field in fields)
		{
			float distance = field.GlobalPosition.DistanceTo(position);
			if (distance < minDistance)
			{
				minDistance = distance;
				nearestField = field;
			}
		}
		
		return nearestField;
	}
	
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			if (mouseEvent.ButtonIndex == MouseButton.Right && mouseEvent.Pressed)
			{
				_lastMousePosition = mouseEvent.Position;
				
				_contextMenu.Position = new Vector2I((int)_lastMousePosition.X, (int)_lastMousePosition.Y);
				_contextMenu.Popup();
			}
		}
	}

	private void InitializeCardEffects()
	{
		// Community cards effects
		cardEffects.Add(("community", 2), (100, null));
		cardEffects.Add(("community", 3), (100, null));
		cardEffects.Add(("community", 4), (-150, null));
		cardEffects.Add(("community", 5), (-50, null));
		cardEffects.Add(("community", 6), (100, null));
		cardEffects.Add(("community", 7), (50, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveToField(5, this);
		}));
		cardEffects.Add(("community", 8), (150, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveToField(28, this);
		}));
		cardEffects.Add(("community", 9), (-100, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveToField(15, this);
		}));
		cardEffects.Add(("community", 10), (200, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveToField(39, this);
		}));
		cardEffects.Add(("community", 11), (200, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveToField(29, this);
		}));
		cardEffects.Add(("community", 12), (200, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveToField(0, this);
		}));
		cardEffects.Add(("community", 13), (-100, null));
		cardEffects.Add(("community", 14), (100, null));
		cardEffects.Add(("community", 15), (200, null));
		cardEffects.Add(("community", 16), (-50, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveToField(27, this);
		}));

		// Chance cards effects
		cardEffects.Add(("chance", 2), (100, null));
		cardEffects.Add(("chance", 3), (150, null));
		cardEffects.Add(("chance", 4), (0, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveToField(0, this);
		}));
		cardEffects.Add(("chance", 5), (0, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveByFields(-2, this);
		}));
		cardEffects.Add(("chance", 6), (100, null));
		cardEffects.Add(("chance", 7), (0, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveToField(0, this);
		}));
		cardEffects.Add(("chance", 8), (0, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveToField(0, this);
		}));
		cardEffects.Add(("chance", 9), (0, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveToField(0, this);
		}));
		cardEffects.Add(("chance", 10), (0, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveToField(0, this);
		}));
		cardEffects.Add(("chance", 11), (-150, null));
		cardEffects.Add(("chance", 12), (-150, null));
		cardEffects.Add(("chance", 13), (-50, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveByFields(-2, this);
		}));
		cardEffects.Add(("chance", 14), (0, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveToField(0, this);
		}));
		cardEffects.Add(("chance", 15), (0, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveToField(0, this);
		}));
		cardEffects.Add(("chance", 16), (0, async () => 
		{
			var currentPlayer = gameManager.getCurrentPlayer();
			await currentPlayer.MoveToField(0, this);
		}));
	}

	private async Task ProcessCardEffect(string cardType, int cardNumber)
	{
		if (cardEffects.TryGetValue((cardType, cardNumber), out var effect))
		{
			// Hide end turn button during effect processing
			endTurnButton.Visible = false;
			
			var currentPlayerIndex = gameManager.GetCurrentPlayerIndex();

			// Apply ECTS effect
			if (effect.ectsEffect != 0)
			{
				gameManager.AddEctsToPlayer(currentPlayerIndex, effect.ectsEffect);
				
				if (effect.ectsEffect > 0)
				{
					GD.Print($"Karta {cardType} {cardNumber}: Gracz otrzymał {effect.ectsEffect} ECTS");
				}
				else
				{
					PlayLostECTSSound();
					GD.Print($"Karta {cardType} {cardNumber}: Gracz stracił {-effect.ectsEffect} ECTS");
				}

				// Small delay for visual feedback
				await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
			}

			// Execute special card effect (pawn movement) if it exists
			if (effect.specialEffect != null)
			{
				try
				{
					await effect.specialEffect();
					// Add delay after special effect
					await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
				}
				catch (Exception e)
				{
					GD.PrintErr($"Błąd podczas wykonywania specjalnego efektu karty: {e.Message}");
				}
			}

			// Show end turn button after all effects are processed
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
			endTurnButton.Visible = true;
		}
	}
	private void PlayLostECTSSound()
	{
		if (lostECTSSoundPlayer != null)
		{
			lostECTSSoundPlayer.Play();
			GD.Print("Odtwarzanie dźwięku stracenia punktów ECTS z karty szansy.");
		}
		else
		{
			GD.PrintErr("Błąd: AudioStreamPlayer3D nie jest zainicjalizowany.");
		}
	}
	private void PlayDeanOfficeSound()
	{
		if (deanOfficeSoundPlayer != null)
		{
			deanOfficeSoundPlayer.Play();
			GD.Print("Odtwarzanie dźwięku pójścia do dziekanatu.");
			achievementManager.Call("track_dean_office_visit");
		}
		else
		{
			GD.PrintErr("Błąd: AudioStreamPlayer3D nie jest zainicjalizowany.");
		}
	}
	public void PublicUtilityFacility(int fieldId, int diceTotal)
	{
		var publicField = (fieldId == 12) ? GetFieldById(12) : GetFieldById(28);
		var otherPublicField = (fieldId == 12) ? GetFieldById(28) : GetFieldById(12);
		
		if (publicField.Owner == gameManager.getCurrentPlayer())
		{
			return;
		}
		
		var multiplier = (publicField.Owner == otherPublicField.Owner) ? 10 : 4;
		var fee = multiplier * diceTotal;
		
		GD.Print("Mnożnik za posiadanie pól: ", multiplier);
		GD.Print($"Wyrzucone oczka: {diceTotal}, opłata: {fee}");
		
		var currentPlayer = gameManager.getCurrentPlayer();
		currentPlayer.SpendECTS(fee);
		gameManager.UpdateECTSUI(gameManager.GetCurrentPlayerIndex());
		
		var publicFieldOwner = publicField.Owner;
		publicFieldOwner.AddECTS(fee);
		gameManager.UpdateECTSUI(publicField.OwnerId);
	}
	public async void StepOnField(int fieldId)
	{
		// Wait for any animations to complete
		await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
		// Hide end turn button initially
		endTurnButton.Visible = false;
		const int goToDeanOfficeFieldId = 30;
		int[] publicUtilities = [12, 28];
		int[] deanOffice = [4, 10, goToDeanOfficeFieldId];
		int[] communityCards = [2, 17, 33];
		int[] chanceCards = [7, 22, 36];
		int[] noActionFields = [0, 4, 10, 20, 38];
		int[] fineFields = [4, 38];

		if (publicUtilities.Contains(fieldId))
		{
			var field = GetFieldById(fieldId);
			GD.Print("Pole użytku publicznego.");
			GD.Print(field.owned);
			if (!field.owned)
			{
				GD.Print("Kupienie publicznego pola z powodu braku właściciela.");
				BuyField(fieldId);
				achievementManager.Call("track_property_purchase", field.Department.ToString(), field.fieldCost, true);
				return;
			}
			GD.Print("Rozliczenie pola.");
			
			gameManager.IsSpecialRollHappening = true;
			
			gameManager.StartDiceRollForCurrentPlayer(DiceRollMode.JustForDisplay);
			await ToSignal(gameManager, GameManager.SignalName.DicesStoppedRolling);
			int diceTotal = gameManager.GetLastDiceTotal();
			PublicUtilityFacility(fieldId, diceTotal);
			
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
			endTurnButton.Visible = true;
		}
		if (deanOffice.Contains(fieldId))
		{
			PlayDeanOfficeSound();
			if (fieldId == goToDeanOfficeFieldId)
			{
				var currentPlayer = gameManager.getCurrentPlayer();
				await currentPlayer.MoveBackward(20, this);
			}
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
			endTurnButton.Visible = true;
		}
		if (communityCards.Contains(fieldId))
		{
			await ShowRandomCard("community");
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
			endTurnButton.Visible = true;
		}
		else if (chanceCards.Contains(fieldId))
		{
			await ShowRandomCard("chance");
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
			endTurnButton.Visible = true;
		}
		else if (noActionFields.Contains(fieldId))
		{
			if (fineFields.Contains(fieldId))
			{
				var currentPlayer = gameManager.getCurrentPlayer();
				currentPlayer.SpendECTS(150);
				gameManager.UpdateECTSUI(gameManager.GetCurrentPlayerIndex());
			}
			await ToSignal(GetTree().CreateTimer(0.5f), "timeout");
			endTurnButton.Visible = true;
		}
		else {
			// Domyślna akcja: zakup pola lub płacenie czynszu
			Figurehead currentFigureHead = gameManager.getCurrentPlayer();
			int currentPosition = currentFigureHead.GetCurrentPositionIndex();
			Field field = gameManager.getCurrentField(currentPosition);

			if (!field.owned)
			{
				BuyField(fieldId);
				achievementManager.Call("track_property_purchase", field.Department.ToString(), field.fieldCost, publicUtilities.Contains(fieldId));
			}
			else if (field.Owner != currentFigureHead)
			{
				if(field.mortgage)
				{
					ShowPopupNotification("This property is on mortgage. You don't have to pay rent ", 3.0f);
				}
				else
				{
					field.PayRent(currentFigureHead, field);
					int rent = field.CheckHouseQuantity(field);
					achievementManager.Call("track_tax_payment", rent);
				}
			}
		}
	}

	public void BuyField(int fieldId)
	{
		buyTime.Start();
		randomCard.Visible = false;
		tradeButton.Disabled = true;
		buildButton.Disabled = true;
		string textureName = $"Field{fieldId}";
		Texture2D fieldTexture = ResourceLoader.Load<Texture2D>($"res://scenes/board/level/textures/{textureName}.png");

		if (fieldTexture != null)
		{
			step_on_card.Texture = fieldTexture;
			Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
			float scaleFactorX = viewportSize.X / 3000f;
			float scaleFactorY = viewportSize.Y / 1250f;
			float scaleFactor = Math.Min(scaleFactorX, scaleFactorY);
			Vector2 scale = new Vector2(scaleFactor, scaleFactor);

			step_on_card.Scale = scale;
			cardView.Scale = scale;
			BuyCard.Visible = true;
		}
	}

	public async Task ShowRandomCard(string cardType)
	{
		randomCard.Visible = false;
		textureDisplay.Visible = false;
		ownerNicknameView.Visible = false;
		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
		float scaleFactorX = viewportSize.X / 2500f;
		float scaleFactorY = viewportSize.Y / 1080f;
		float scaleFactor = Math.Min(scaleFactorX, scaleFactorY);
		Vector2 scale = new Vector2(scaleFactor, scaleFactor);

		randomCard.Scale = scale;
		Random random = new Random();
		int cardNumber = random.Next(2, 17);

		string initialTexturePath = $"res://scenes/board/level/textures/{(cardType == "community" ? "community_chest/community_1" : "chances/chance_1")}.png";
		var cardTexture = ResourceLoader.Load<Texture2D>(initialTexturePath);

		if (cardTexture != null)
		{
			randomCard.Texture = cardTexture;
			randomCard.Visible = true;

			Tween tween = CreateTween();
			tween.TweenProperty(randomCard, "rotation_degrees", 360 * 2, 1.2f)
				.SetTrans(Tween.TransitionType.Circ)
				.SetEase(Tween.EaseType.InOut);

			await ToSignal(tween, "finished");

			randomCard.RotationDegrees = 0;

			string finalTexturePath = $"res://scenes/board/level/textures/{(cardType == "community" ? "community_chest/community" : "chances/chance")}_{cardNumber}.png";
			cardTexture = ResourceLoader.Load<Texture2D>(finalTexturePath);
			randomCard.Texture = cardTexture;
			randomCard.Visible = true;

			await ProcessCardEffect(cardType, cardNumber);
		}
	}

public async void ShowFieldTexture(int fieldId)
	{
		randomCard.Visible = false;
		textureDisplay.Visible = false;

		string textureName = $"Field{fieldId}";
		Texture2D fieldTexture = ResourceLoader.Load<Texture2D>($"res://scenes/board/level/textures/{textureName}.png");

		if (fieldTexture != null)
		{
			textureDisplay.Texture = fieldTexture;
			textureDisplay.Position = new Vector2(400, 0);
			Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
			float scaleFactorX = viewportSize.X / 3000f;
			float scaleFactorY = viewportSize.Y / 1250f;
			float scaleFactor = Math.Min(scaleFactorX, scaleFactorY);
			Vector2 scale = new Vector2(scaleFactor, scaleFactor);
			field = gameManager.getCurrentField(fieldId);
			if(field.owned == true)
			{
				string nickname = field.GetUserNickname(field);
				ownerNickname.Text = $"właściciel:{nickname}";
				if(field.mortgage)
					mortgageInfo.Text = "Pole jest aktualnie oddane pod zastaw.";
			}
			else
			{
				ownerNickname.Text = "Pole nie ma właściciela";
			}
			ownerNickname.Visible = true;
			ownerNicknameView.Visible = true;
			ownerNicknameView.Position = new Vector2(890, 280);
			textureDisplay.Scale = new Vector2(0, 0);
			textureDisplay.Visible = true;

			Tween tween = CreateTween();
			tween.TweenProperty(textureDisplay, "scale", scale, 0.15f)
				.SetTrans(Tween.TransitionType.Linear)
				.SetEase(Tween.EaseType.InOut);

			await ToSignal(tween, "finished");
		}
	}
	
	public void HideFieldTexture()
{
	Tween tween = CreateTween();
	tween.TweenProperty(textureDisplay, "scale", Vector2.Zero, 0.15f)
		.SetTrans(Tween.TransitionType.Linear)
		.SetEase(Tween.EaseType.InOut);
   
	ownerNickname.Visible = false;
	ownerNicknameView.Visible = false;
	
	tween.TweenCallback(Callable.From(() => {
		textureDisplay.Visible = false;
	}));
}

	public async Task MovePawn(Figurehead pawn, int fieldId, int positionIndex)
	{
		Vector3? targetPosition = GetPositionForPawn(fieldId, positionIndex);
		if (targetPosition.HasValue)
		{
			Tween tween = CreateTween();
			tween.TweenProperty(pawn, "global_position", targetPosition.Value, 0.5f)
				.SetTrans(Tween.TransitionType.Linear)
				.SetEase(Tween.EaseType.InOut);
			await ToSignal(tween, "finished");
		}
	}

	public Vector3? GetPositionForPawn(int fieldId, int positionIndex)
	{
		Field field = fields.Find(f => f.FieldId == fieldId);
		if (field != null && positionIndex >= 0 && positionIndex < field.positions.Count)
		{
			return field.positions[positionIndex];
		}
		return null;
	}

	public Field GetFieldById(int fieldId)
	{
		return fields.Find(field => field.FieldId == fieldId);
	}

	public List<Field> GetFields()
	{
		return fields;
	}

	
	public Godot.Collections.Array GetFieldsOwnedByPlayerName(string playerName)
	{
		Godot.Collections.Array result = new Godot.Collections.Array();
		
		foreach (Field field in fields)
		{
			if (field.owned && field.Owner != null && field.Owner.Name == playerName)
			{
				var fieldInfo = new Godot.Collections.Dictionary();
				fieldInfo.Add("id", field.FieldId);
				fieldInfo.Add("name", field.Name);
				result.Add(fieldInfo);
			}
		}
		
		return result;
	}
}
