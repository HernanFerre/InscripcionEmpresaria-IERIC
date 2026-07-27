using System;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Newtonsoft.Json;


namespace IERIC.SumariosIERIC.Application.Helper;
public class Functions
{
    public async Task Run(MinioClient minio, string bucketName, string carpeta, string nombre, string tipo, byte[] data)
    {
        var beArgs = new BucketExistsArgs()
            .WithBucket(bucketName);
        bool found = await minio.BucketExistsAsync(beArgs).ConfigureAwait(false);
        if (!found)
        {
            var mbArgs = new MakeBucketArgs()
                .WithBucket(bucketName);
            await minio.MakeBucketAsync(mbArgs).ConfigureAwait(false);
        }
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(carpeta + "/" + nombre)
            .WithObjectSize(data.Length)
            .WithStreamData(new MemoryStream(data))
            .WithContentType(tipo);

        await minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
    }

    public async Task<FileStreamResult> RunGet(MinioClient minio, string carpetaId, string archivoId)
    {

        MemoryStream retorno = new MemoryStream();

        StatObjectArgs statObjectArgs = new StatObjectArgs()
                                      .WithBucket("sumarios")
                                      .WithObject(carpetaId + "/" + archivoId);


        var existe = await minio.StatObjectAsync(statObjectArgs);

        GetObjectArgs getObjectArgs = new GetObjectArgs()
                                             .WithBucket("sumarios")
                                            .WithObject(carpetaId + "/" + archivoId)
                                             .WithCallbackStream(async (stream) =>
                                             {
                                                 stream.CopyTo(retorno);
                                             });
        var salida = await minio.GetObjectAsync(getObjectArgs);


        retorno.Position = 0;

        return new FileStreamResult(retorno, salida.ContentType);

    }
}