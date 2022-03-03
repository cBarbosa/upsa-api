using System.Threading.Tasks;

namespace upsa_api.Services.Interfaces
{
    public interface IThemisService
    {
        Task<ThemisService.Processo> GetProcess(string number);
        Task<bool> AddProcessFoward(string number, ThemisService.AndamentoProcessoInput andamento);
        Task<ThemisService.Processo.Pessoa> GetPerson(int personId);
    }
}