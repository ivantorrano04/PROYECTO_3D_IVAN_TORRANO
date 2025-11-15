using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    public GameObject handUI;
    public GameObject UIText;

    public GameObject invKey;
    public GameObject fadeFX;

    public string nextSceneName; // Name of the next scene to load


    private bool inReach = false; // Inicializado a false


    void Start()
    {
        Debug.Log("Start: Inicializando puerta. inReach = " + inReach);
        if (handUI != null) handUI.SetActive(false); else Debug.LogError("Start: handUI es null");
        if (UIText != null) UIText.SetActive(false); else Debug.LogError("Start: UIText es null");

        if (invKey != null) invKey.SetActive(false); else Debug.LogError("Start: invKey es null");

        if (fadeFX != null) fadeFX.SetActive(false); else Debug.LogError("Start: fadeFX es null");

        Debug.Log("Start: Objetos UI desactivados.");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter: Colisionador " + other.name + " entró en el trigger. Tag: " + other.tag);
        if (other.gameObject.CompareTag("Reach")) // Usar CompareTag es más eficiente
        {
            inReach = true;
            Debug.Log("OnTriggerEnter: inReach ahora es TRUE.");
            if (handUI != null) 
            {
                handUI.SetActive(true);
                Debug.Log("OnTriggerEnter: handUI activado.");
            }
            else 
            {
                Debug.LogError("OnTriggerEnter: handUI es null, no se puede activar.");
            }
        }
        else
        {
            Debug.Log("OnTriggerEnter: El objeto no tiene el tag 'Reach'. No se hace nada.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerExit: Colisionador " + other.name + " salió del trigger. Tag: " + other.tag);
        if (other.gameObject.CompareTag("Reach"))
        {
            inReach = false;
            Debug.Log("OnTriggerExit: inReach ahora es FALSE.");
            if (handUI != null) 
            {
                handUI.SetActive(false);
                Debug.Log("OnTriggerExit: handUI desactivado.");
            }
            else 
            {
                Debug.LogError("OnTriggerExit: handUI es null, no se puede desactivar.");
            }
            
            if (UIText != null) 
            {
                UIText.SetActive(false);
                Debug.Log("OnTriggerExit: UIText desactivado.");
            }
            else 
            {
                Debug.LogError("OnTriggerExit: UIText es null, no se puede desactivar.");
            }
        }
        else
        {
            Debug.Log("OnTriggerExit: El objeto no tiene el tag 'Reach'. No se hace nada.");
        }
    }

    void Update()
    {
        // Debug constante para ver el estado de inReach y los estados activos de los objetos UI
        // Descomentar la siguiente línea si necesitas un log muy frecuente (puede ser verboso)
        // Debug.Log("Update: Estado actual - inReach: " + inReach + ", invKey Activo: " + (invKey != null ? invKey.activeInHierarchy.ToString() : "null"));

        if (inReach && Input.GetButtonDown("Interact"))
        {
            Debug.Log("Update: Detectado Input.GetButtonDown('Interact') mientras inReach es TRUE.");
            
            if (invKey != null)
            {
                Debug.Log("Update: Estado de invKey.activeInHierarchy: " + invKey.activeInHierarchy);
                
                if (!invKey.activeInHierarchy) // No tiene la llave
                {
                    Debug.Log("Update: No se tiene la llave. Mostrando mensaje de error.");
                    if (handUI != null) 
                    {
                        handUI.SetActive(true);
                        Debug.Log("Update: handUI activado (mensaje de error).");
                    }
                    if (UIText != null) 
                    {
                        UIText.SetActive(true);
                        Debug.Log("Update: UIText activado (mensaje de error).");
                    }
                }
                else // Tiene la llave
                {
                    Debug.Log("Update: Se tiene la llave. Iniciando transición de escena.");
                    if (handUI != null) 
                    {
                        handUI.SetActive(false);
                        Debug.Log("Update: handUI desactivado (antes de fade).");
                    }
                    if (UIText != null) 
                    {
                        UIText.SetActive(false);
                        Debug.Log("Update: UIText desactivado (antes de fade).");
                    }
                    if (fadeFX != null) 
                    {
                        fadeFX.SetActive(true);
                        Debug.Log("Update: fadeFX activado.");
                    }
                    // Iniciar la corrutina de finalización
                    if(fadeFX != null) // Asegurarse que fadeFX existe antes de iniciar la corrutina
                    {
                         StartCoroutine(ending());
                         // Para evitar que se llame de nuevo mientras se ejecuta la corrutina
                         // Deshabilitamos el collider temporalmente o usamos una bandera.
                         // Por simplicidad aquí, deshabilitamos el collider.
                         this.GetComponent<Collider>().enabled = false; 
                         Debug.Log("Update: Collider de la puerta deshabilitado temporalmente para evitar múltiples activaciones.");
                    }
                    else
                    {
                        Debug.LogError("Update: fadeFX es null, no se puede iniciar la transición de escena.");
                    }
                    
                }
            }
            else
            {
                Debug.LogError("Update: invKey es null, no se puede comprobar si se tiene la llave.");
            }
        }
    }

    IEnumerator ending()
    {
        Debug.Log("Coroutine ending: Iniciada. Esperando 0.6 segundos...");
        yield return new WaitForSeconds(.6f);
        Debug.Log("Coroutine ending: Espera completada. Cargando escena: " + nextSceneName);
        
        if (!string.IsNullOrEmpty(nextSceneName))
        {
             SceneManager.LoadScene(nextSceneName);
             Debug.Log("Coroutine ending: SceneManager.LoadScene llamado.");
        }
        else
        {
            Debug.LogError("Coroutine ending: nextSceneName está vacío o es nulo. No se puede cargar la escena.");
        }
        
    }
}