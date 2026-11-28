import 'package:medlink/Features/ProviderProfile/Domain/Entites/provider_profile.dart';

abstract class ProviderRepository {
  Future<List<ProviderProfile>> getProviders({int pageSize = 10});

  Future<bool> isSlotAvailable(String providerId, DateTime slot);

  Future<void> book(String providerId, DateTime slot);
}
