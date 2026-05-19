using Helpers;
using System.Threading.Tasks;

namespace Services
{
    public class GameSavesServiesBase
    {
        public virtual void Initialize()
        {

        }

        protected virtual async Task LoadSaved()
        {
            await AsyncHelper.Skip();
        }

        public virtual async Task UploadSaves()
        {
            await AsyncHelper.Skip();
        }
    }
}