using AutoMoqCore;
using Moq;
using NUnit.Framework;

namespace EL.InfluxDB.UnitTests
{
    public abstract class With_an_automocked<T>
    {
        public T ClassUnderTest
        {
            get
            {
                if (classUnderTest == null)
                {
                    classUnderTest = mocker.Create<T>();
                }
                return classUnderTest;
            }
        }
        private T classUnderTest;
        private AutoMoqer mocker;

        [SetUp]
        public void WithAnAutoMockedSetup()
        {
            mocker = new AutoMoqer();
        }

        [TearDown]
        public void WithAnAutoMockedTearDown()
        {
            classUnderTest = default;
        }

        protected Mock<TMock> GetMock<TMock>() where TMock : class
        {
            return mocker.GetMock<TMock>();
        }

        protected TAny IsAny<TAny>()
        {
            return It.IsAny<TAny>();
        }
    }
}