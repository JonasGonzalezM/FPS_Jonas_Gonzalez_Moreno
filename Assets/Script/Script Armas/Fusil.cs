using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Fusil : MonoBehaviour
{
    //Bala
    public GameObject bala;


    //Fuerza de la Bala
    public float shootForce, upwardForce;


    //Estadisticas 
    public float timeBetweenShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;

    int bulletsLeft, bulletsShot;



    //Booleanos
    bool shooting, readyToShoot, reloading;

    //Referencias
    public Camera fpsCam;
    public Transform attackPoint;

    //Graficas
    public GameObject muzzleFlash;
    public TextMeshProUGUI ammunitionDisplay;

    //Arreglos de Bugs
    public bool allowInvoke = true;


    private void Awake()
    {
        //Asgurarse de que el cargador este lleno
        bulletsLeft = magazineSize;
        readyToShoot = true;

    }


    private void Update()
    {

        MyInput();


        //Configurar el Display de la municion, si existe
        if(ammunitionDisplay != null)
        {
            ammunitionDisplay.SetText(bulletsLeft / bulletsPerTap + " / " + magazineSize / bulletsPerTap);
        }


    }



    private void MyInput()
    {

        //Comprobar que se permita el mantener el boton y tomar el input correspondiente
        if (allowButtonHold)
        {
            shooting = Input.GetKey(KeyCode.Mouse0);
        }
        else
        {
            shooting = Input.GetKeyDown(KeyCode.Mouse0);
        }

        //Recargar
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading)
        {
            Reload();
        }
        //Recargar automaticamente cuando intente disparar sin municion
        if(readyToShoot && shooting && !reloading && bulletsLeft <= 0)
        {
            Reload();
        }


        //Disparo
        if(readyToShoot && shooting && !reloading &&bulletsLeft > 0)
        {
            //Setear las balas a 0
            bulletsShot = 0;
            Shoot();
        }



    }



    private void Shoot()
    {
        readyToShoot = false;

        //Encontrar el punto exacto de impacto usando un Raycast
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); //Esto es un rayo que pasa por el medio de la pantalla
        RaycastHit hit;

        //Comprobar si el rayo le da a algo
        Vector3 targetPoint;
        if(Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(75); //Esto es solo un punto alejado del jugador

        }

        //Calcular la direccion del PundoAtaque (attackPoint) al PuntoObjetivo (targetPoint)
        //La direccion de A a B es = B.position - A.position
        Vector3 directionWithoutSpread = targetPoint - attackPoint.position;

        //Calcular la dispersion
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        //Calcular una nueva direccion con dispersion
        Vector3 directionWitheSpread = directionWithoutSpread + new Vector3(x,y, 0); //Esto añade dispersion en la ultima direccion

        //Instanciar la bala la cual se queda guardada en la linea de codigo que esta justo abajo
        GameObject currentBullet = Instantiate(bala, attackPoint.position, Quaternion.identity);
        //Rotar la baka para disparar en la direccion
        currentBullet.transform.forward = directionWitheSpread.normalized;


        //Añadir fuerzas a la bala
        currentBullet.GetComponent<Rigidbody>().AddForce(directionWitheSpread.normalized * shootForce,ForceMode.Impulse);
        currentBullet.GetComponent<Rigidbody>().AddForce(fpsCam.transform.up * upwardForce,ForceMode.Impulse);

        //Instanciar el muzzle flash si se tiene uno
        if(muzzleFlash != null)
        {
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);
        }


        //Invocar la funcion resetShot (si no ha sido invocada ya)
        if(allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShooting);
            allowInvoke = false;

            //la funcion de invocar se puede usar de esta manera Invoke("FunctionName",2f) es que llama a la funcion tras 2 segundos

        }

        //Si quieres mas balas por Click asegurarse de repetir la funcion de Disparo
        if(bulletsShot < bulletsPerTap && bulletsLeft > 0)
        {
            Invoke("Shoot", timeBetweenShots);
        }


        bulletsLeft--;
        bulletsShot++;



    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowInvoke = true;

    }


    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);

    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }


}
