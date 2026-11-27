abstract class ProviderRepository {
  Future<bool> isSlotAvailable(String providerId, DateTime slot);

  Future<void> book(String providerId, DateTime slot);
}
