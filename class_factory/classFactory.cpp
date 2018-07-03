#include "stdafx.h"

#include <ql/indexes/ibor/libor.hpp>
#include <ql/indexes/ibor/audlibor.hpp>
#include <ql/indexes/ibor/cadlibor.hpp>
#include <ql/indexes/ibor/chflibor.hpp>
#include <ql/indexes/ibor/dkklibor.hpp>
#include <ql/indexes/ibor/gbplibor.hpp>
#include <ql/indexes/ibor/jpylibor.hpp>
#include <ql/indexes/ibor/nzdlibor.hpp>
#include <ql/indexes/ibor/seklibor.hpp>

#include <ql/indexes/inflation/all.hpp>

#include <boost/any.hpp>
#include <boost/shared_ptr.hpp>
#include <boost/algorithm/string.hpp>
#include <map>

#include "classFactory.h"
#include "QuanlibDepositLiborRates.h"
#include "QuantlibSwapRates.h"
#include "Calendars.h"

using namespace __QLCPP;

#define stringify( name ) # name

// calendar.xml
ObjectFactory<QuantLib::Calendar>::ClassNameFactoryMap ObjectFactory<QuantLib::Calendar>::factoryMap;
void ObjectFactory<QuantLib::Calendar>::InitObjectFactory()
{
	RegisterClass<QuantLib::Argentina>();
	RegisterClass<QuantLib::Australia>();
	RegisterClass<QuantLib::BespokeCalendar>();
	RegisterClass<QuantLib::Brazil>();
	RegisterClass<QuantLib::Brazil, QuantLib::Brazil::Market>(QuantLib::Brazil::Settlement, stringify(Settlement));
	RegisterClass<QuantLib::Brazil, QuantLib::Brazil::Market>(QuantLib::Brazil::Exchange, stringify(Exchange));
	RegisterClass<QuantLib::Canada>();
	RegisterClass<QuantLib::Canada, QuantLib::Canada::Market>(QuantLib::Canada::Settlement, stringify(Settlement));
	RegisterClass<QuantLib::Canada, QuantLib::Canada::Market>(QuantLib::Canada::TSX, stringify(TSX));
}

// enumbasis.xml
ObjectFactory<QuantLib::DayCounter>::ClassNameFactoryMap ObjectFactory<QuantLib::DayCounter>::factoryMap;
void ObjectFactory<QuantLib::DayCounter>::InitObjectFactory()
{
	RegisterClass<QuantLib::Actual360>();
	RegisterClass<QuantLib::Actual365Fixed>();
	RegisterClass<QuantLib::ActualActual>();
	RegisterClass<QuantLib::ActualActual, QuantLib::ActualActual::Convention>(QuantLib::ActualActual::ISMA, stringify(ISMA));
	RegisterClass<QuantLib::ActualActual, QuantLib::ActualActual::Convention>(QuantLib::ActualActual::Bond, stringify(Bond));
	RegisterClass<QuantLib::ActualActual, QuantLib::ActualActual::Convention>(QuantLib::ActualActual::ISDA, stringify(ISDA));
	RegisterClass<QuantLib::ActualActual, QuantLib::ActualActual::Convention>(QuantLib::ActualActual::Historical, stringify(Historical));
	RegisterClass<QuantLib::ActualActual, QuantLib::ActualActual::Convention>(QuantLib::ActualActual::Actual365, stringify(Actual365));
	RegisterClass<QuantLib::ActualActual, QuantLib::ActualActual::Convention>(QuantLib::ActualActual::AFB, stringify(AFB));
	RegisterClass<QuantLib::ActualActual, QuantLib::ActualActual::Convention>(QuantLib::ActualActual::Euro, stringify(Euro));
	RegisterClass<QuantLib::Business252>();
	RegisterClass<QuantLib::OneDayCounter>();
	RegisterClass<QuantLib::Thirty360>();
	RegisterClass<QuantLib::Thirty360, QuantLib::Thirty360::Convention>(QuantLib::Thirty360::USA, stringify(USA));
	RegisterClass<QuantLib::Thirty360, QuantLib::Thirty360::Convention>(QuantLib::Thirty360::BondBasis, stringify(BondBasis));
	RegisterClass<QuantLib::Thirty360, QuantLib::Thirty360::Convention>(QuantLib::Thirty360::European, stringify(European));
	RegisterClass<QuantLib::Thirty360, QuantLib::Thirty360::Convention>(QuantLib::Thirty360::EurobondBasis, stringify(EurobondBasis));
	RegisterClass<QuantLib::Thirty360, QuantLib::Thirty360::Convention>(QuantLib::Thirty360::Italian, stringify(Italian));
	
}

// currency.xml
ObjectFactory<QuantLib::Currency>::ClassNameFactoryMap ObjectFactory<QuantLib::Currency>::factoryMap;
void ObjectFactory<QuantLib::Currency>::InitObjectFactory()
{
	RegisterClass<QuantLib::ARSCurrency>();
	RegisterClass<QuantLib::ATSCurrency>(); 
	RegisterClass<QuantLib::AUDCurrency>();
	RegisterClass<QuantLib::BDTCurrency>(); 
	RegisterClass<QuantLib::BEFCurrency>();
	RegisterClass<QuantLib::BGLCurrency>();
	RegisterClass<QuantLib::BRLCurrency>();
	RegisterClass<QuantLib::BYRCurrency>();
	RegisterClass<QuantLib::CADCurrency>();
	RegisterClass<QuantLib::CHFCurrency>();
	RegisterClass<QuantLib::CLPCurrency>();
	RegisterClass<QuantLib::CNYCurrency>();
	RegisterClass<QuantLib::COPCurrency>();
}
