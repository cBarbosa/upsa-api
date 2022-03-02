using System.Threading.Tasks;

namespace upsa_api.Services
{
    public interface IThemisService
    {
        Task<ThemisService.Processo> GetProcess(string number);
        Task<bool> AddProcessFoward(string number, ThemisService.AndamentoProcessoInput andamento);
    }


}