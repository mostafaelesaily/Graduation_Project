import 'package:medlink/Core/Network/api_client.dart';
import 'package:medlink/Core/Network/token_storage.dart';
import 'package:medlink/Features/Auth/Data/datasources/auth_remote_data_source.dart';
import 'package:medlink/Features/Auth/Data/repos/auth_repository_impl.dart';
import 'package:medlink/Features/Auth/Domain/Repos/auth_repository.dart';

class Dependencies {
  late final TokenStorage tokenStorage;
  late final ApiClient apiClient;
  late final AuthRemoteDataSource authRemoteDataSource;
  late final AuthRepository authRepository;

  Dependencies() {
    tokenStorage = TokenStorage();

    apiClient = ApiClient(
      tokenStorage: tokenStorage,
      baseUrl: 'http://192.168.1.4:7048',
    );

    authRemoteDataSource = AuthRemoteDataSource(client: apiClient);

    authRepository = AuthRepositoryImpl(
      remoteDataSource: authRemoteDataSource,
      tokenStorage: tokenStorage,
    );
  }
}
