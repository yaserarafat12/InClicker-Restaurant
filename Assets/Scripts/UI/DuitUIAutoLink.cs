using UnityEngine;
using UnityEngine.UI;

namespace InUniverse.InResto
{
    /// <summary>
    /// Pasang script ini di komponen Text (untuk Saldo Duit).
    /// Dia bakal otomatis nyari VisualManager dan nempelin dirinya ke situ.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class DuitUIAutoLink : MonoBehaviour
    {
        private void Start()
        {
            Text myText = GetComponent<Text>();
            
            // Cari VisualManager di scene
            VisualManager vm = FindObjectOfType<VisualManager>();
            
            if (vm != null)
            {
                vm.moneyText = myText;
                Debug.Log("<color=green>UI Linker: Saldo Text linked to VisualManager!</color>");
            }
            else
            {
                // Kalau belum ada, coba lagi nanti (mungkin VisualManager belum dibuat Bootstrapper)
                Invoke("TryLink", 0.5f);
            }
        }

        private void TryLink()
        {
            VisualManager vm = FindObjectOfType<VisualManager>();
            if (vm != null)
            {
                vm.moneyText = GetComponent<Text>();
                Debug.Log("<color=green>UI Linker: Saldo Text linked (Late)!</color>");
            }
        }
    }
}
