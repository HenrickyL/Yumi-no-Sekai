using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

public class HexUnit : MonoBehaviour {

	const float rotationSpeed = 180f;
	public  float TravelSpeed {
		get{
			return  status.Speed;
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
			var response = new List<HexCell>(){
				location.GetNeighbor(HexDirection.W),
				location.GetNeighbor(HexDirection.NW),
				location.GetNeighbor(HexDirection.NE),
				location.GetNeighbor(HexDirection.E)
			};
			
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
			if(value == null){
				oldTargets = targets;
				targets = null;
			}
			else if(value.Where(u => AttackPath.Contains(u.Location)).Any()){
				targets = value;
				oldTargets = targets;
			}else{
				targets = oldTargets;
			}
		}
	}

	StatusBar _statusBar;
	UnitStatus status= new UnitStatus();
	
	public static HexUnit unitPrefab;
	private int speed=1;
	public int Speed { get{return speed;} }

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
			return orientation;
		}
		set {
			orientation = value;
			transform.localRotation = Quaternion.Euler(0f, value, 0f);
		}
	}

	float orientation;

	List<HexCell> pathToTravel;


	private void Awake() {
		_statusBar = gameObject.GetComponentInChildren<StatusBar>();
		status.Default();
		RefreshStatusBar();
	}

	private void RefreshStatusBar(){
		_statusBar.SetStatus(status);
	}

	public void ValidateLocation () {
		transform.localPosition = location.Position;
	}

	public bool IsValidFullDestination (HexCell cell) {
		return !cell.IsUnderwater && !cell.Unit;
	}
	public bool IsValidDestination (HexCell cell) {
		return !cell.IsUnderwater && !cell.Unit
				&& ( location.Elevation-1 <= cell.Elevation &&  cell.Elevation <= location.Elevation+1 );
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
		yield return LookAt(pathToTravel[1].Position);

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
		orientation = transform.localRotation.eulerAngles.y;
		ListPool<HexCell>.Add(pathToTravel);
		pathToTravel = null;
	}

	IEnumerator LookAt (Vector3 point) {
		point.y = transform.localPosition.y;
		Quaternion fromRotation = transform.localRotation;
		Quaternion toRotation =
			Quaternion.LookRotation(point - transform.localPosition);
		float speed = rotationSpeed / Quaternion.Angle(fromRotation, toRotation);

		for (
			float t = Time.deltaTime * speed;
			t < 1f;
			t += Time.deltaTime * speed
		) {
			transform.localRotation =
				Quaternion.Slerp(fromRotation, toRotation, t);
			yield return null;
		}

		transform.LookAt(point);
		orientation = transform.localRotation.eulerAngles.y;
	}

	public void Die () {
		location.Unit = null;
		Destroy(gameObject);
	}

	public void Save (BinaryWriter writer) {
		location.coordinates.Save(writer);
		writer.Write(orientation);
	}

	public static void Load (BinaryReader reader, HexGrid grid) {
		HexCoordinates coordinates = HexCoordinates.Load(reader);
		float orientation = reader.ReadSingle();
		grid.AddUnit(
			Instantiate(unitPrefab), grid.GetCell(coordinates), orientation
		);
	}

	void OnEnable () {
		if (location) {
			transform.localPosition = location.Position;
		}
		RefreshStatusBar();
	}
	private void LateUpdate() {
		var cam = Camera.main.transform.transform.forward;
		var position = this.transform.position;
		var adjust = new Vector3(position.x+cam.x,position.y,position.z+cam.z);
		transform.LookAt( adjust);
		RefreshStatusBar();
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
		if(targets.Any()){
			foreach(var t in targets){
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

	

}