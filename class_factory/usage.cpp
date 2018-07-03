#include "classFactory.h"

void MarshalString ( String ^ s, std::string& os ) 
{
	using namespace Runtime::InteropServices;
	const char* chars = 
		(const char*)(Marshal::StringToHGlobalAnsi(s)).ToPointer();
	os = chars;
	Marshal::FreeHGlobal(IntPtr((void*)chars));
}

#define OBJECT_FACTORY(VAR, BASE_NAME, CLASS_NAME) \
{\
	try\
	{\
		std::string __tmp;\
		MarshalString(CLASS_NAME, __tmp);\
		VAR = static_cast<BASE_NAME*>(IntPtr(__QLCPP::ObjectFactory<BASE_NAME>::ObjectCreator(__tmp)).ToPointer());\
	}\
	catch (...)\
	{\
		throw gcnew Exception("ObjectFactory exception on: " + CLASS_NAME);\
	}\
}

void QuantLibAdaptor::Init()
{
	__QLCPP::ObjectFactory<QuantLib::Calendar>::InitObjectFactory();
	__QLCPP::ObjectFactory<QuantLib::DayCounter>::InitObjectFactory();
	__QLCPP::ObjectFactory<QuantLib::Currency>::InitObjectFactory();
}

// ... usage

{
	QuantLib::Calendar* calendar = NULL;
			
	if (!String::IsNullOrEmpty(ycd->settings->Calendar->ClassName))
	{
		if (!String::IsNullOrEmpty(ycd->settings->Calendar->MarketName)){
			OBJECT_FACTORY(calendar, QuantLib::Calendar, gcnew String(ycd->settings->Calendar->ClassName+"::"+ycd->settings->Calendar->MarketName));}
		else
			OBJECT_FACTORY(calendar, QuantLib::Calendar, gcnew String(ycd->settings->Calendar->ClassName));
	}
	else
		OBJECT_FACTORY(calendar, QuantLib::Calendar, gcnew String("UnitedKingdom"));
}