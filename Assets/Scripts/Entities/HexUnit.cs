using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

public class HexUnit : MonoBehaviour {

	public Animator animator;
	public Transform body;
	AnimationType animationType;
	HexDirectionAll animationDirection;
	const float rotationSpeed = 180f;
	public  float TravelSpeed {
		get{
			return  status.Speed*3;
		}
	}
	public List<HexCell> MovePath { 
		get{
			var response = location.GetNeighborPerNivel(status.Speed, c=>IsValidDestination(c));
			return response;
		} 
	}
	public List<HexCell> AttackPath { 
		get{
			var response = location.GetTriangleByDirection(direction,status.Range,c=>IsValidCellAttack(c,status.Range));
			return response;
		} 
	}
	List<HexUnit> targets;
	List<HexUnit> oldTargets;

	public List<HexUnit> OldTargets { get{return oldTargets;} }
	public List<HexUnit> Targets {
		get {
			return targets;
		}
		set{
			if(value == null || !value.Any()){
				oldTargets = targets;
				targets = null;
			}
			else{
				targets = value;
				oldTargets = targets;
			}
		}
	}

	StatusBar _statusBar;
	UnitStatus status;
	public UnitStatus Status { get{return status;} }
	
	public static HexUnit unitPrefab;
	public HexCell Location {
		get {
			return location;
		}
		set {
			if (location) {
				location.Unit = null;
			}
			oldLocation=location;
			location = value;
			value.Unit = this;
			transform.localPosition = value.Position;
		}
	}
	public HexCell OldLocation {
		get {
			return oldLocation;
		}
	}

	HexCell location;
	HexCell oldLocation;


	public float Orientation {
		get {
			return ((int)direction+1)*60f;
		}
		set {
			direction = (HexDirection)(value/60f);
			transform.localRotation = Quaternion.Euler(0f, Orientation, 0f);
		}
	}

	HexDirection direction;
	List<HexCell> pathToTravel;


	private void OnEnable() {
		_statusBar = gameObject.GetComponentInChildren<StatusBar>();
		status= new UnitStatus();
		status.Default();
		
		RefreshStatusBar();
		animationType = AnimationType.Idle;
		animationDirection = HexDirectionAll.N;
		if (location) {
			transform.localPosition = location.Position;
		}
	}

	private void RefreshStatusBar(){
		_statusBar.SetStatus(status);
	}

	public void ValidateLocation () {
		transform.localPosition = location.Position;
	}

	public bool IsValidFullDestination (HexCell cell) {
		return cell && !cell.IsUnderwater && !cell.Unit;
	}
	public bool IsValidDestination (HexCell cell) {
		return  cell && !cell.IsUnderwater && !cell.Unit
				&& ( location.Elevation-1 <= cell.Elevation &&  cell.Elevation <= location.Elevation+1 );
	}
	public bool IsValidCellAttack (HexCell cell, int range=1) {
		return  cell &&( location.Elevation-range <= cell.Elevation &&  cell.Elevation < location.Elevation+range );
	}

	public void Travel (List<HexCell> path) {
		Location = path[path.Count - 1];
		pathToTravel = path;
		StopAllCoroutines();
		StartCoroutine(TravelPath());

	}

	IEnumerator TravelPath () {
		Vector3 a, b, c = pathToTravel[0].Position;
		transform.localPosition = c;
		SwapAnimationType(AnimationType.Walk);
		// yield return LookAt(pathToTravel[1].Position);

		float t = Time.deltaTime * TravelSpeed;
		for (int i = 1; i < pathToTravel.Count; i++) {
			a = c;
			b = pathToTravel[i - 1].Position;
			c = (b + pathToTravel[i].Position) * 0.5f;
			for (; t < 1f; t += Time.deltaTime * TravelSpeed) {
				transform.localPosition = Bezier.GetPoint(a, b, c, t);
				Vector3 d = Bezier.GetDerivative(a, b, c, t);
				d.y = 0f;
				transform.localRotation = Quaternion.LookRotation(d);
				yield return null;
			}
			t -= 1f;
		}

		a = c;
		b = pathToTravel[pathToTravel.Count - 1].Position;
		c = b;
		for (; t < 1f; t += Time.deltaTime * TravelSpeed) {
			transform.localPosition = Bezier.GetPoint(a, b, c, t);
			Vector3 d = Bezier.GetDerivative(a, b, c, t);
			d.y = 0f;
			transform.localRotation = Quaternion.LookRotation(d);
			yield return null;
		}

		transform.localPosition = location.Position;
		Orientation = transform.localRotation.eulerAngles.y;
		ListPool<HexCell>.Add(pathToTravel);
		pathToTravel = null;
		SwapAnimationType(AnimationType.Idle);
	}
	

	// IEnumerator LookAt (Vector3 point) {
	// 	point.y = transform.localPosition.y;
	// 	Quaternion fromRotation = transform.localRotation;
	// 	Quaternion toRotation =
	// 		Quaternion.LookRotation(point - transform.localPosition);
	// 	float speed = rotationSpeed / Quaternion.Angle(fromRotation, toRotation);

	// 	for (
	// 		float t = Time.deltaTime * speed;
	// 		t < 1f;
	// 		t += Time.deltaTime * speed
	// 	) {
	// 		transform.localRotation =
	// 			Quaternion.Slerp(fromRotation, toRotation, t);
	// 		yield return null;
	// 	}

	// 	transform.LookAt(point);
	// 	Orientation = transform.localRotation.eulerAngles.y;
	// }

	public void Die () {
		// location.Unit = null;
		// Destroy(gameObject);
	}

	public void Save (BinaryWriter writer) {
		location.coordinates.Save(writer);
	}

	public static void Load (BinaryReader reader, HexGrid grid) {
		HexCoordinates coordinates = HexCoordinates.Load(reader);
		float orientation = reader.ReadSingle();
		grid.AddUnit(
			Instantiate(unitPrefab), grid.GetCell(coordinates), orientation
		);
	}

	private void LateUpdate() {
		CamAjust();
		RefreshStatusBar();
	}


    void CamAjust(){
		
		var cam = Camera.main.transform.position;
		var position = transform.position;
		var adjust = new Vector3(cam.x, position.y,cam.z);
		body.LookAt(adjust);
		UpdateAnimationDirection(cam);
	}

	public bool IsValidMoveDestination(HexCell cell){
		return MovePath.Contains(cell);
	}
	public bool IsValidAttackDestination(HexCell cell){
		return AttackPath.Contains(cell);
	}
	public bool IsOldTarget(HexUnit unit){
		return AttackPath.Contains(unit.Location);
	}

	public void ShakeUnit(){
		Vector3 position = this.transform.position;
		//https://forum.unity.com/threads/shake-an-object-from-script.138382/
	}

	public void BasicAttackTargets(){
		if(Targets !=null && Targets.Any()){
			foreach(var t in Targets){
				t.TakeDamage(CalcDamageNormalAttack(this));
			}
		}
	}

	float CalcDamageNormalAttack(HexUnit unit){
		return status.Strength*(1-status.Defense/100);
	}

	public void TakeDamage(float value){
		this.status.HP = (int)Math.Round(status.HP - value);
		RefreshStatusBar();
	}

	void UpdateAnimationDirection(Vector3 camPos){
		var position = transform.position;
		var pos = new Vector2(position.x, position.z);
		var cam = new Vector2(camPos.x, camPos.z);
		var v1 =  pos -Vector2.up;
		var v2 =  pos - cam;
		float angle = Mathf.Atan2(v2.x, v2.y) * Mathf.Rad2Deg;
		animationDirection = angle.HalfCircleToDirection();
		SwapAnimationType(animationType);
	}

	void SwapAnimationType(AnimationType type){
		if(type != animationType){
			Debug.Log($"State{animationType}");
			Debug.Log($"Idle {animator.GetBool($"Idle")} - Walk {animator.GetBool($"Walk")} - Die {animator.GetBool($"Die")} - Attack {animator.GetBool($"Attack")}");
			animator.SetBool($"Idle",false);
			animator.SetBool($"Walk",false);
			animator.SetBool($"Die",false);
			animator.SetBool($"Attack",false);
			animationType = type;
			animator.SetBool($"{animationType}",true);
		}
		animator.SetBool($"{animationType}N", 
				animationDirection.IsFront());
		animator.SetBool($"{animationType}S",
				animationDirection.IsBack());
	}
	

}