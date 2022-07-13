using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexGameUI : MonoBehaviour {

	public HexGrid grid;
	float waitTime = 1f;
	HexCell currentCell, hoverCell, previosCurrentCell, destinationCell;
	private Color colorHover = new Color(0,0,0,0.15f),
		 colorSelectedUnity = new Color(0,0,1,0.3f),
		 colorMove = new  Color(0.98f,0.83f,0.29f,0.5f),
		 colorMoveHovered = Color.white,
		 colorSelected = Color.white,
		 colorAttack = new Color(1,0,0,0.5f),
		 colorAttackHovered = Color.red,
		 colorSelectedUnityHevered = Color.blue;
	Color colorActiveAction, colorActiveActionHovered;
	private List<Color> actionsColors, actionColorHovered;
	public bool IsAttackMode { get{return unitActionType == UnitActionsEnum.Attack;} }
	public bool IsMoveMode { get{return unitActionType == UnitActionsEnum.Move;} }
	HexUnit selectedUnit, previosSelected;
	bool inAction = false;
	UnitActionsEnum unitActionType= UnitActionsEnum.Move;
	
	public void SetEditMode (bool toggle) {
		enabled = !toggle;
		grid.ShowUI(!toggle);
		grid.ClearPath();
	}

	void SetActionMode(int action){
		unitActionType = (UnitActionsEnum)action;
	}

	private void OnEnable() {
		actionsColors = new List<Color>(){
			colorMove,colorAttack
		};
		actionColorHovered = new List<Color>(){
			colorMoveHovered, colorAttackHovered
		};
		SetMoveMode();
	}
	void Update () {
		if (!EventSystem.current.IsPointerOverGameObject()) {
			HoveredCell();
			// CheckTravelEnd();
			UpdateCurrentCell();
			if (Input.GetMouseButtonDown(0)) {
				if(!selectedUnit){
					DoSelection();
				}else{
					if(IsMoveMode){
						DoMove(currentCell);
					}else if(IsAttackMode){
						DoNormalAttack(currentCell);
					}
				}
			}
			else if(Input.GetKeyUp(KeyCode.Space) && selectedUnit){
				DoAreaAttack();
			}
			else if(Input.GetKey(KeyCode.LeftShift)){
				DoPathfinding();
			}else if(inAction){
				grid.ClearPath();
				inAction = false;
			}
		}
	}

    

    void SetHighlightHover(HexCell prev, HexCell next, Color? prevColor, Color nextColor){
		if(prevColor== null || prevColor ==  Color.clear){
			prev.DisableHighlight();
		}else{
			prev.EnableHighlight(prevColor ?? Color.red);
		}
		hoverCell = next;
		hoverCell.EnableHighlight(nextColor);
	}
	void HoveredCell(){
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		HexCell previous = hoverCell;
		if(!previous){
			hoverCell = grid.GetCell(ray);
			return;
		}
		HexCell next = grid.GetCell(ray);
		if( hoverCell && previous && next && next != previous ){
			HighlightSelectUnitAction();
			if(selectedUnit){
				bool prevInSelect = IsAttackMode?
					selectedUnit.IsValidAttackDestination(previous):
					selectedUnit.IsValidMoveDestination(previous);
				bool nextInSelect = IsAttackMode?
					selectedUnit.IsValidAttackDestination(next):
					selectedUnit.IsValidMoveDestination(next);
				if(prevInSelect && next == selectedUnit.Location){
					SetHighlightHover(previous,next,colorActiveAction, colorSelectedUnityHevered);
				}
				else if(prevInSelect && nextInSelect){
					SetHighlightHover(previous,next,colorActiveAction, colorActiveActionHovered);
				}
				else if(!prevInSelect && nextInSelect){
					SetHighlightHover(previous,next,null, colorActiveActionHovered);
				}
				else if(prevInSelect && !nextInSelect){
					SetHighlightHover(previous,next,colorActiveAction, colorHover);
				}
				else if(previous == selectedUnit.Location && nextInSelect){
					SetHighlightHover(previous,next,
						IsAttackMode? colorSelected:colorSelectedUnity,
						IsAttackMode && next.Unit? colorAttackHovered: colorMoveHovered);
				}
				else{
					SetHighlightHover(previous,next,null, colorHover);
				}
			}
			else{
				SetHighlightHover(previous,next,null, colorHover);
			}
			
		}
	}

	void DoSelection () {
		grid.ClearPath();
		UpdateCurrentCell();
		if(currentCell ){
			ClearPreviosSelectUnit(selectedUnit);
			if(currentCell.Unit != selectedUnit){
				SwapSelectedUnit(currentCell.Unit);
				HighlightSelectUnitAction();
			}
		}
	}
	void HighlightSelectUnitAction(){
		if(selectedUnit){
			selectedUnit.Location.EnableHighlight(colorSelectedUnity);
			if(unitActionType == UnitActionsEnum.Move){
				foreach(var cell in selectedUnit.MovePath){
					cell.EnableHighlight(colorActiveAction);
				}
			}else{
				foreach(var cell in selectedUnit.AttackPath){
					if(cell.Unit)
						cell.EnableHighlight(colorActiveActionHovered);
					else
						cell.EnableHighlight(colorActiveAction);
				}
			}
		}
	}
	void ClearPreviosSelectUnit(HexUnit unit){
		if(unit){
			unit.Location.DisableHighlight();
			previosCurrentCell?.DisableHighlight();
			if(unitActionType == UnitActionsEnum.Move){
				foreach(var c in unit.MovePath){
					c.DisableHighlight();
				}
			}else{
				foreach(var c in unit.AttackPath){
					c.DisableHighlight();
				}
			}
		}
	}
	

	void DoPathfinding (HexCell destination) {
		inAction = true;
		grid.ClearPath();
		if (currentCell && selectedUnit &&
			selectedUnit.IsValidFullDestination(destination) &&
			selectedUnit.IsValidMoveDestination(destination)) 
		{
			grid.FindPath(selectedUnit.Location, destination, 10*selectedUnit.Speed);
		}
		else {
			grid.ClearPath();
		}
	}
	void DoPathfinding () {
		grid.ClearPath();
		if (currentCell) 
		{
			grid.FindPath(selectedUnit.Location, currentCell, 10*selectedUnit.Speed);
		}
		else {
			grid.ClearPath();
		}
	}

	void DoMove (HexCell cell) {
		if (cell) {
			destinationCell = cell;
			DoPathfinding(cell);
			if(grid.HasPath){
				ClearPreviosSelectUnit(selectedUnit);
				selectedUnit.Travel(grid.GetPath());
				grid.ClearPath();
			}
			SwapSelectedUnit(null);
			destinationCell = null;
		}
	}
	private void DoNormalAttack(HexCell cell){
		if(cell){
			StartCoroutine(DoNormalAttackAsync(cell));
		}else{
			SwapSelectedUnit(null);
			SetMoveMode();
		}
	}
	
	private IEnumerator DoNormalAttackAsync(HexCell cell)
    {
        if(cell && cell.Unit && selectedUnit.IsValidAttackDestination(cell)) {
			selectedUnit.Targets = new List<HexUnit>(){
				cell.Unit
			};
			HighlightAttack();
			yield return new WaitForSeconds(waitTime);
			selectedUnit.BasicAttackTargets();
		}
		SwapSelectedUnit(null);
		SetMoveMode();
		
    }
	private void DoAreaAttack(){
		StartCoroutine(DoAreaAttackAsync());
		SwapSelectedUnit(null);
		SetMoveMode();
	}
	private IEnumerator DoAreaAttackAsync()
    {
		if(selectedUnit){
			selectedUnit.Targets = selectedUnit.AttackPath.Where(c=>c.Unit!= null).Select(c=>c.Unit).ToList();
			HighlightAttack();
			yield return new WaitForSeconds(waitTime);
			selectedUnit.BasicAttackTargets();
			SwapSelectedUnit(null);
			SetMoveMode();
		}
		
    }
	
	void HighlightAttack(){
		if(selectedUnit){
			ClearPreviosSelectUnit(selectedUnit);
			selectedUnit.Location.EnableHighlight(Color.white);
			foreach(var cell in selectedUnit.AttackPath){
				cell.EnableHighlight(colorAttack);
			}
			foreach(var unit in selectedUnit.Targets){
				unit.Location.EnableHighlight(Color.gray);
			}
		}
	}

	void SwapSelectedUnit(HexUnit unit){
		ClearPreviosSelectUnit(selectedUnit);
		previosSelected = selectedUnit;
		selectedUnit = unit;
		HighlightSelectUnitAction();
	}
	void CheckTravelEnd(){
		if(selectedUnit && destinationCell){
			if(selectedUnit.Location == destinationCell){
				HighlightSelectUnitAction();
				destinationCell = null;
			}
		}
	}
	bool UpdateCurrentCell () {
		HexCell cell =
			grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
		if (cell != currentCell ) {
			previosCurrentCell = currentCell;
			currentCell = cell;
			return true;
		}
		return false;
	}

	public void SetAttackMode(){
		if(selectedUnit){
			ClearPreviosSelectUnit(selectedUnit);
		}
		colorActiveAction = actionsColors[1];
		colorActiveActionHovered = actionColorHovered[1];
		unitActionType = UnitActionsEnum.Attack;
	}
	public void SetMoveMode(){
		if(selectedUnit){
			ClearPreviosSelectUnit(selectedUnit);
		}
		colorActiveAction = actionsColors[0];
		colorActiveActionHovered = actionColorHovered[0];
		unitActionType = UnitActionsEnum.Move;
	}

	

	
}