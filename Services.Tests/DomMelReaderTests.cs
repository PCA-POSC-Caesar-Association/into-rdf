﻿using Common.ProvenanceModels;
using Services.TransformationServices.SpreadsheetTransformationServices;
using Services.GraphParserServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Parsing;
using Xunit;

namespace Services.Tests
{
    public class DomMelReaderTests
    {
        private readonly IEnumerable<ISpreadsheetTransformationService> _transformationServices;
        private readonly IGraphParser _graphParser;

        public DomMelReaderTests(IEnumerable<ISpreadsheetTransformationService> transformationServices, IGraphParser graphParser)
        {
            _transformationServices = transformationServices;
            _graphParser = graphParser;
        }

        [Fact]
        public void TestDomParsing()
        {
            var testFile = "TestData/test.xlsx";
            var testTrainFile = "TestData/testTrain.ttl";
            var rdfTestUtils = new RdfTestUtils(DataSource.Mel);
            var stream = File.Open(testFile, FileMode.Open, FileAccess.Read);

            var melTransformationService = _transformationServices.FirstOrDefault(service => service.GetDataSource() == DataSource.Mel) ?? 
                                                throw new ArgumentException($"Transformer of type {DataSource.Mel} not available");

            using FileStream fs = new FileStream(testTrainFile, FileMode.Open, FileAccess.Read);
            using StreamReader sr = new StreamReader(fs, Encoding.UTF8);

            string trainContent = sr.ReadToEnd();

            var trainRevisionModel = _graphParser.ParseRevisionTrain(trainContent);

            var resultGraph = melTransformationService.Transform(trainRevisionModel, stream);

            Assert.NotNull(resultGraph);

            var graph = new Graph();
            var parser = new TurtleParser();
            parser.Load(graph, new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(resultGraph))));

            //Actual Data
            rdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/test/A1"),
                new Uri("https://rdf.equinor.com/source/mel#Header3"),
                "1729"
            );

            rdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/test/A1"),
                new Uri("https://rdf.equinor.com/source/mel#Header4"),
                "3300.375"
            );

            rdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/test/A499"),
                new Uri("https://rdf.equinor.com/source/mel#Header55"),
                "BC500"
            );

            rdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/test/A498"),
                new Uri("https://rdf.equinor.com/source/mel#Header55"),
                "BC499"
            );

            rdfTestUtils.AssertTripleAsserted(
                graph,
                new Uri("https://rdf.equinor.com/test/A497"),
                new Uri("https://rdf.equinor.com/source/mel#Header53"),
                "BA498"
            );
        }


    }
}
