using System.Threading.Tasks;
using JHipsterNet.Core.Pagination;
using ProcurandoApartamento.Domain.Services.Interfaces;
using ProcurandoApartamento.Domain.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ProcurandoApartamento.Domain.Services
{
    public class ApartamentoService : IApartamentoService
    {
        protected readonly IApartamentoRepository _apartamentoRepository;

        public ApartamentoService(IApartamentoRepository apartamentoRepository)
        {
            _apartamentoRepository = apartamentoRepository;
        }

        public virtual async Task<Apartamento> Save(Apartamento apartamento)
        {
            await _apartamentoRepository.CreateOrUpdateAsync(apartamento);
            await _apartamentoRepository.SaveChangesAsync();
            return apartamento;
        }

        public virtual async Task<IPage<Apartamento>> FindAll(IPageable pageable)
        {
            var page = await _apartamentoRepository.QueryHelper()
                .GetPageAsync(pageable);
            return page;
        }

        public virtual async Task<Apartamento> FindOne(long id)
        {
            var result = await _apartamentoRepository.QueryHelper()
                .GetOneAsync(apartamento => apartamento.Id == id);
            return result;
        }

        public virtual async Task Delete(long id)
        {
            await _apartamentoRepository.DeleteByIdAsync(id);
            await _apartamentoRepository.SaveChangesAsync();
        }

        public virtual async Task<string> FindBetterApartment(string[] parameters)
        {
            parameters = parameters.Select(p => p.ToUpperInvariant()).ToArray();

            var list = await _apartamentoRepository.FindBetterApartment(parameters);

            var groupList = list.GroupBy(p => new { p.Quadra }, (key, group) => new
            {
                Quadra = key.Quadra,
                Result = group.ToList()
            });

            var itemsToReturn = groupList.Where(p => p.Result.Count == parameters.Length);

            if (itemsToReturn.Count() > 1)
            {
                return $@"QUADRA {itemsToReturn.Select(p => p.Quadra).LastOrDefault()}";
            }
            else if (itemsToReturn.Count() == 1)
            {
                return $@"QUADRA {itemsToReturn.Select(p => p.Quadra).FirstOrDefault()}";
            }
            else
            {
                var strinResult = "";

                /*
                 Confesso que fiquei com d�vida na regra 3. N�o sabia se era para retornar apenas um apartamento,
                ou todos os apartamentos conforme a prioriza��o. Acabei retornando todos conforme a prioriza��o.

                A Prioridade de estabelecimentos depende da ordem de entrada de dados.
                Ent�o se a entrada de dados for ACADEMIA e ESCOLA por exemplo,
                deve-se priorizar apartamentos mais pr�ximos de ACADEMIAS e depois de ESCOLAS;
                 
                 */

                foreach (var item in parameters)
                {
                    var apartments = list.Where(p => p.Estabelecimento == item).ToList();

                    if (apartments.Count() > 1)
                    {
                        strinResult += $@"QUADRA {apartments.Select(p => p.Quadra).LastOrDefault()} ";
                    }
                    else if (apartments.Count() == 1)
                    {
                        strinResult += $@"QUADRA {apartments.Select(p => p.Quadra).FirstOrDefault()} ";
                    }
                }

                if (!string.IsNullOrEmpty(strinResult))
                {
                    strinResult = strinResult.Remove(strinResult.Length - 1);
                }

                return strinResult;
            }
        }
    }
}
