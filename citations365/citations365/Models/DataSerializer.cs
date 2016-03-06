using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace citations365.Models {
    public class DataSerializer<DataType> {
        public static async Task SaveObjectsAsync(DataType sourceData, String targetFileName) {
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(targetFileName, CreationCollisionOption.ReplaceExisting);
            var outStream = await file.OpenStreamForWriteAsync(); // ERREUR NON GEREE ICI?

            DataContractSerializer serializer = new DataContractSerializer(typeof(DataType));
            serializer.WriteObject(outStream, sourceData);
            await outStream.FlushAsync();
            outStream.Dispose();
        }

        public static async Task<DataType> RestoreObjectsAsync(string filename) {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(filename);

            var inStream = await file.OpenStreamForReadAsync();

            //Deserialize the objetcs
            DataContractSerializer serializer = new DataContractSerializer(typeof(DataType));
            DataType data = (DataType)serializer.ReadObject(inStream);
            inStream.Dispose();

            return data;
        }
    }
}
