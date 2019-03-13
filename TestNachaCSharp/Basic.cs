using System;
using Xunit;
using System.Runtime.CompilerServices;
using System.IO;
using NachaSharp;
using FSharp.Data.FlatFileMeta;
using System.Linq;
            
namespace TestNachaCSharp
{
    public class Basic
    {

        private string LoadFile(string fileName, [CallerFilePath] string sourceFilePath = "")
        {
            var dir = Path.GetDirectoryName(sourceFilePath);
            var path = Path.Combine(dir, "..", "TestNachaSharp", "Data", fileName);
            using(var stream = File.OpenRead(path))
            using(var reader = new StreamReader(stream)){
                return  reader.ReadToEnd();
            }          
        }
        
        private void CompareLines (TextReader expectedReader, TextReader actualReader) {
            var actualNext = actualReader.ReadLine();
            var expectedNext = expectedReader.ReadLine();
            var i = 1;
            while(!(actualNext is null) && !(expectedNext is null)){
                var actual = $"Line {i}: {actualNext}";
                var expected =  $"Line {i}: {expectedNext}";
                Assert.Equal(expected, actual);
                actualNext = actualReader.ReadLine();
                expectedNext = expectedReader.ReadLine();
                i = i + 1;
            }
        }
        private FileHeaderRecord ParseFile(string fileName, [CallerFilePath] string sourceFilePath = "")
        {    
            var dir = Path.GetDirectoryName(sourceFilePath);
            var path = Path.Combine(dir, "..", "TestNachaSharp", "Data", fileName);
            using(var stream = File.OpenRead(path)){
                var maybeHeader = NachaFile.ParseFile(stream);
                if (maybeHeader.TrySomeRow(out var header)){
                    return header;
                }
                throw new Exception("Parse Failure");
            }            
        }
      
        [Fact]
        public void CSharpReadFile()
        {
            var header = ParseFile("20110729A.ach.txt");
            var count = header.Batches.SelectMany(it=>it.Entries).Count();
            Assert.True(count > 100);
        }

        [Fact]
        public void ConstructFile(){
            var header =  FileHeaderRecord.Create("DummyDest","DummyOrig", "B");
            var batch = BatchHeaderRecord.Create("DUM",
                                                    "Company Name DUM", 
                                                    "COMP DUM",
                                                    "DuM",
                                                    "COMENtryd",
                                                    new DateTime(2018,1,1),
                                                    "D",
                                                    "dfi dum");
                                
            Assert.Equal("1",header.RecordTypeCode);
        }
        
        [Fact]  
        public void CompareAfterWriting (){
            var filename = "transactions1.ach.txt";
            using(var mem = new MemoryStream()){
                var parsed =  ParseFile(filename);
                NachaFile.WriteFile(parsed, mem, "\r");
                mem.Position = 0L;
                using(var reader = new StreamReader(mem))
                using(var expect = new StringReader(LoadFile(filename))){
                    CompareLines(expect, reader);
                }
            }
        }
    }
}
