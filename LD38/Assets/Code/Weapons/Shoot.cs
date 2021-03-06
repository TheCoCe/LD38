﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Shoot : MonoBehaviour
{
  protected TeamPlayer teamPlayer;
  protected Transform bulletSpawnAnchorPointOnGun;

  const float maxHoldTime = 2;
  public float shootHoldTime;

  public abstract bool showShootPower
  {
    get;
  }

  public float shootPower
  {
    get
    {
      var _shootHoldTime = shootHoldTime % (maxHoldTime * 2);
      if(_shootHoldTime > maxHoldTime)
      {
        _shootHoldTime = maxHoldTime * 2 - _shootHoldTime;
      }
      return _shootHoldTime / maxHoldTime;
    }
  }

  protected virtual void Start()
  {
    teamPlayer = transform.root.GetComponent<TeamPlayer>();
    bulletSpawnAnchorPointOnGun = transform.FindChild("BulletSpawn");
  }

  protected virtual void OnFireStart()
  {

  }

  protected virtual void OnFireStay()
  {

  }

  protected virtual void OnFireStop()
  {

  }

  protected void FixedUpdate()
  {
    if(teamPlayer.isMyTurn == false || TurnController.phase != TurnController.Phase.Shoot)
    {
      return;
    }

    Aim();

    if(Input.GetAxis("Fire1") > 0)
    {
      if(shootHoldTime == 0)
      {
        OnFireStart();
      }
      OnFireStay();
      shootHoldTime += Time.deltaTime;
    }
    else 
    {
      OnFireStop();
      shootHoldTime = 0;
    }
  } 

  void Aim()
  {
    Vector2 screenPos = Input.mousePosition;
    if(screenPos.x < Screen.width / 2)
    {
      screenPos += new Vector2(Screen.width / 2, 0);
    }

    Ray targetRay = Camera.main.ScreenPointToRay(screenPos);
    Plane plane = new Plane(transform.root.forward, transform.root.position);
    
    float distance;
    if(plane.Raycast(targetRay, out distance))
    {
      Vector3 mousePosition = targetRay.GetPoint(distance);
     
      Vector3 delta =  transform.position - mousePosition;
      Vector3 up = transform.position - Vector3.zero;

      Quaternion originalRotation = transform.rotation;
      transform.rotation = Quaternion.LookRotation(delta, up);

      
      //Vector3 euler = transform.localRotation.eulerAngles;
      if((transform.right - transform.root.forward).sqrMagnitude > 1
        //Mathf.Abs(euler.y) > 1 || Mathf.Abs(euler.z) > 1 || 
        //euler.x > 75 || euler.x < -20
        )
      {
        transform.rotation = originalRotation;
      }
    }
  }
  
  /// <param name="antiAccurancyInDegrees">Higher means worse aim</param>
  protected void FireProjectile(Projectile resource, float antiAccurancyInDegrees)
  {
    Quaternion rng = Quaternion.Euler(UnityEngine.Random.Range(-antiAccurancyInDegrees, antiAccurancyInDegrees),
      UnityEngine.Random.Range(-antiAccurancyInDegrees, antiAccurancyInDegrees),
      UnityEngine.Random.Range(-antiAccurancyInDegrees, antiAccurancyInDegrees));
    Projectile newBullet = Instantiate(resource, bulletSpawnAnchorPointOnGun.transform.position, transform.rotation * rng);
    newBullet.shooter = gameObject.transform.root.gameObject;
    newBullet.shootPower = shootPower;
  }
}
