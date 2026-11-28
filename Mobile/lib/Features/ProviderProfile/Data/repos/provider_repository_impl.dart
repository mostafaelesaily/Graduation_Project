import 'package:medlink/Features/ProviderProfile/Data/datasources/provider_remote_data_source.dart';
import 'package:medlink/Features/ProviderProfile/Domain/Entites/provider_profile.dart';
import 'package:medlink/Features/ProviderProfile/Domain/Repos/provider_repository.dart';

class ProviderRepositoryImpl implements ProviderRepository {
  final ProviderRemoteDataSource remoteDataSource;

  ProviderRepositoryImpl({required this.remoteDataSource});

  @override
  Future<List<ProviderProfile>> getProviders({int pageSize = 10}) =>
      remoteDataSource.getProviders(pageSize: pageSize);

  @override
  Future<bool> isSlotAvailable(String providerId, DateTime slot) =>
      remoteDataSource.isSlotAvailable(providerId, slot);

  @override
  Future<void> book(String providerId, DateTime slot) =>
      remoteDataSource.book(providerId, slot);
}
