using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using OpenAutoBench_ng.Communication.Radio.Motorola.XCMPRadioBase;

namespace OpenAutoBench_ng.UnitTests.RadioTests.MotorolaXCMPBase
{
    internal class TestXCMPRadioBase
    {

        private XCMPRadioTestInterface intf;
        private MotorolaXCMPRadioBase radio;
        [SetUp]
        public void Setup()
        {
            intf = new XCMPRadioTestInterface();
            radio = new MotorolaXCMPRadioBase(intf);
            radio.Connect(underTest: true);
        }

        [Test]
        public void TestServiceMode()
        {
            intf.FlushBuffers();
            byte[] reference = { 0x00, 0x02, 0x00, 0x0c };
            byte[] reply = { 0x00, 0x03, 0x80, 0x0c, 0x00 };
            intf.SetInBuffer(reply);
            radio.EnterServiceMode();
            
            byte[] test = intf.ReadOutBuffer();
            Assert.AreEqual(reference, test);

        }

        [Test]
        public void TestSerialNumber()
        {
            intf.FlushBuffers();
            // basic return with 123ABC1234 serial number
            byte[] reply = { 0x00, 0x0e, 0x80, 0x0e, 0x00, 0x08, 0x31, 0x32, 0x33, 0x41, 0x42, 0x43, 0x31, 0x32, 0x33, 0x34};
            intf.SetInBuffer(reply);
            byte[] retval = radio.GetStatus(StatusOperation.SerialNumber);
            byte[] reference = { 0x00, 0x03, 0x00, 0x0e, 0x08 };
            byte[] refRetval = { 0x31, 0x32, 0x33, 0x41, 0x42, 0x43, 0x31, 0x32, 0x33, 0x34 };
            byte[] test = intf.ReadOutBuffer();
            Assert.AreEqual(reference, test);
            Assert.AreEqual(refRetval, retval);

        }

        [Test]
        public void TestModelNumber()
        {
            intf.FlushBuffers();
            // basic return with H91TGD9PW7AN model number
            byte[] reply = { 0x00, 0x15, 0x80, 0x0E, 0x00, 0x07, 0x48, 0x39, 0x31, 0x54, 0x47, 0x44, 0x39, 0x50, 0x57, 0x37, 0x41, 0x4E, 0x00, 0x00, 0x00, 0x00, 0x00 };
            intf.SetInBuffer(reply);
            byte[] retval = radio.GetStatus(StatusOperation.ModelNumber);
            byte[] reference = { 0x00, 0x03, 0x00, 0x0e, 0x07 };
            byte[] refRetval = { 0x48, 0x39, 0x31, 0x54, 0x47, 0x44, 0x39, 0x50, 0x57, 0x37, 0x41, 0x4E, 0x00, 0x00, 0x00, 0x00, 0x00 };
            byte[] test = intf.ReadOutBuffer();
            Assert.AreEqual(reference, test);
            Assert.AreEqual(refRetval, retval);

        }
    }
}
