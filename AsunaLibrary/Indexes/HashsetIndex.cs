using AsunaLocalSearch.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsunaLocalSearch.Indexes
{
    [Serializable]
    public partial class HashsetIndex
    {
        internal Dictionary<string, List<HashRecord>> Records { get; set; }

        //TODO fazer ler diretorios e arquivos recursivamente e indexar as palavras
        //fazer criar varios arquivos de hash com o binary formatter pra pesquisar em todos paralelamente
        //identificar no proprio nome de arquivo oss hash menor e maior q zero: minus zero: idx0.m0, idx1.m0, idx2.m0 Plus zero: idx0.p0, idx1.p0,

        //no caso de indexar conteudo de palavras, o key string acima vai ser o diretorio e arquivo.
        //no caso de indexar somente nome de arquivos... o diretorio completo com o nome do arquivo vai ser o hash, enquanto que a key pode ser a unidade, C:\, D:\...

        //OU:
        //a hash pode ser so o nome do arquivo, enquanto a key o diretorio, porque o cara pode querer buscar só no diretorio X que foi indexado, ou um nivel acima,
        //ou um nivel abaixo

        //MAS:
        //o cara pode querer pesquisar conteudo do arquivo e tambem filtrar por diretorio

        //da´pi a string key sendo o diretorio completo com o nome do arquivo resolve ambos os problemas
    }

    public partial class HashsetIndex
    {
        public static async Task<HashsetIndex> CreateIndexOfFileNamesAsync(string path, string searchPattern, CancellationToken cancellationToken = default)
        {
            var results = new List<FileNameAndPath>();

            var t = Task.Run(() =>
            {
                results = FileSystemHelper.Search(path, searchPattern);
            }, cancellationToken);

            await t;

           /* if (cancellationToken.IsCancellationRequested)
                return await Task.FromResult(results);*/

            results = results.OrderBy(x => x.PathHash).ToList();

            var records = new Dictionary<string, List<HashRecord>>();

            int pathHash = 0;

            foreach (var r in results)
            {
                if (pathHash != r.PathHash)
                {
                    pathHash = r.PathHash;
                    records.Add(r.Path, new List<HashRecord>());
                    records[r.Path].Add(new HashRecord { Hash = r.Filename.GetHashCode(StringComparison.InvariantCultureIgnoreCase) });
                }
                else
                    records[r.Path].Add(new HashRecord { Hash = r.Filename.GetHashCode(StringComparison.InvariantCultureIgnoreCase) });
            }
            return new HashsetIndex { Records = records };
        }

        public static async Task<HashsetIndex> CreateIndexOfFileContentAsync(string path, string searchPattern, CancellationToken cancellationToken = default)
        {
            var results = new List<FileNameAndPath>();

            var t = Task.Run(() =>
            {
                results = FileSystemHelper.Search(path, searchPattern);
            }, cancellationToken);

            await t;

          /*  if (cancellationToken.IsCancellationRequested)
                return await Task.FromResult(results);*/

            ConcurrentDictionary<string, List<HashRecord>> index = new ConcurrentDictionary<string, List<HashRecord>>();

            results.AsParallel()
                .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                .WithCancellation(cancellationToken)
                .ForAll(async arquivo =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    var hashes = new List<HashRecord>();

                    string fullPath = Path.Combine(arquivo.Path, arquivo.Filename);

                    using (var fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var bs = new BufferedStream(fs))
                    using (var sr = new StreamReader(bs, Encoding.UTF8, true))
                    {
                        var linha = await sr.ReadLineAsync();
                        var palavras = linha.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        foreach(var p in palavras)
                            hashes.Add(new HashRecord { Hash = p.ToLowerInvariant().GetHashCode() });
                    }

                    index[fullPath] = hashes;

                    //Path.Combine(f.Path, f.Filename)
                });
            return null;
            //return index;

        }
    }



    internal struct HashRecord
    {
        //TODO adicionar o hash do arquivo com extensão e tambem sem extensao
        
        internal int Hash { get; set; }
    }
}
