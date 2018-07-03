#pragma once

#pragma managed (push, off)

namespace __QLCPP 
{
	template <class B>
	class ObjectFactory
	{
		typedef std::map<std::string, boost::shared_ptr<B> >  ClassNameFactoryMap;
		static ClassNameFactoryMap factoryMap;

		template <class D>
		static void RegisterClass()
		{
			std::string key (typeid(D).name());
			boost::algorithm::to_lower(key);
			factoryMap.insert(std::make_pair(key, boost::shared_ptr<D>(new D)));
		}

		template <class D, class P>
		static void RegisterClass(P param, char* paramAsStr)
		{
			std::ostringstream tmp;
			tmp.str("");
			tmp << typeid(D).name() << "::" << paramAsStr << std::ends;

			std::string key (tmp.str().c_str());
			boost::algorithm::to_lower(key);
			factoryMap.insert(std::make_pair(key, boost::shared_ptr<D>(new D(param))));
		}

	public:
		
		static B* ObjectCreator( std::string className )
		{
			// search QuantLib classes

			std::string key("class QuantLib::");
			
			if(className.size() != 0)
				 key += className;

			boost::algorithm::to_lower(key);
			ClassNameFactoryMap::iterator factoryPos(factoryMap.find(key));

			if ( factoryPos != factoryMap.end() )
			{
				return factoryPos->second.get();
			}
			
			// search Custom QuantLib classes

			key = "class __QLCPP::";

			if(className.size() != 0)
				 key += className;

			boost::algorithm::to_lower(key);
			factoryPos = factoryMap.find(key);

			if ( factoryPos != factoryMap.end() )
			{
				return factoryPos->second.get();
			}
			
			// no factory found...
			throw std::runtime_error("Attempt to dynamically request instance of unsupported class type");
		}

		static void InitObjectFactory();

		// Inflation Index
		static boost::shared_ptr<B> CreateInflationIndexFactory(std::string& className);
	};
}

#pragma managed (pop)



