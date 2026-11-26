import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:medlink/Features/Auth/Data/models/register_request_model.dart';
import 'package:medlink/Features/Auth/Domain/Repos/auth_repository.dart';
import 'package:medlink/Features/Auth/Presentation/Signup/Cubit/signup_state.dart';

class SignupCubit extends Cubit<SignupState> {
  final AuthRepository authRepository;

  SignupCubit({required this.authRepository}) : super(const SignupState());

  Future<void> signup(RegisterRequestModel model) async {
    if (state.status == SignupStatus.loading) return;

    emit(state.copyWith(status: SignupStatus.loading, clearError: true));

    try {
      await authRepository.register(model);
      emit(state.copyWith(status: SignupStatus.success, clearError: true));
    } on Exception catch (e) {
      emit(
        state.copyWith(
          status: SignupStatus.failure,
          errorMessage: _getErrorMessage(e),
        ),
      );
    }
  }

  String _getErrorMessage(Object error) {
    final message = error.toString();

    if (message.startsWith('Exception: ')) {
      return message.substring('Exception: '.length);
    }

    return 'Signup failed. Please try again.';
  }

  void reset() {
    emit(const SignupState());
  }
}
