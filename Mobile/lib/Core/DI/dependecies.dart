import 'package:medlink/Core/Network/api_client.dart';
import 'package:medlink/Core/Network/token_storage.dart';
import 'package:medlink/Features/Appointments/Data/datasources/appointments_remote_data_source.dart';
import 'package:medlink/Features/Appointments/Data/repos/appointments_repository_impl.dart';
import 'package:medlink/Features/Appointments/Domain/Repos/appointments_repository.dart';
import 'package:medlink/Features/Auth/Data/datasources/auth_remote_data_source.dart';
import 'package:medlink/Features/Auth/Data/repos/auth_repository_impl.dart';
import 'package:medlink/Features/Auth/Domain/Repos/auth_repository.dart';
import 'package:medlink/Features/ProviderProfile/Data/datasources/provider_remote_data_source.dart';
import 'package:medlink/Features/ProviderProfile/Data/repos/provider_repository_impl.dart';
import 'package:medlink/Features/ProviderProfile/Domain/Repos/provider_repository.dart';

class Dependencies {
  late final TokenStorage tokenStorage;
  late final ApiClient apiClient;
  late final AuthRemoteDataSource authRemoteDataSource;
  late final AuthRepository authRepository;
  late final ProviderRepository providerRepository;
  late final AppointmentsRepository appointmentsRepository;

  Dependencies() {
    tokenStorage = TokenStorage();

    apiClient = ApiClient(
      tokenStorage: tokenStorage,
        baseUrl: 'http://192.168.1.5:7048',
    );

    authRemoteDataSource = AuthRemoteDataSource(client: apiClient);

    authRepository = AuthRepositoryImpl(
      remoteDataSource: authRemoteDataSource,
      tokenStorage: tokenStorage,
    );

    providerRepository = ProviderRepositoryImpl(
      remoteDataSource: ProviderRemoteDataSource(client: apiClient),
    );

    appointmentsRepository = AppointmentsRepositoryImpl(
      remoteDataSource: AppointmentsRemoteDataSource(client: apiClient),
    );
  }
}
