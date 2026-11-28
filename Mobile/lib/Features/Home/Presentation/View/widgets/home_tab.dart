import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Features/Appointments/Domain/Entites/appointment.dart';
import 'package:medlink/Features/Appointments/Presentation/Cubit/appointments_cubit.dart';
import 'package:medlink/Features/Appointments/Presentation/Cubit/appointments_state.dart';
import 'package:medlink/Features/Home/Presentation/Models/home_models.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/ai_navigator_card.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/care_services_grid.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/home_greeting.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/home_header.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/top_providers_section.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/upcoming_care_section.dart';
import 'package:go_router/go_router.dart';
import 'package:medlink/Features/Home/Presentation/Cubit/home_providers_cubit.dart';
import 'package:medlink/Features/Home/Presentation/Cubit/home_providers_state.dart';
import 'package:medlink/Features/ProviderProfile/Domain/Repos/provider_repository.dart';

class HomeTab extends StatelessWidget {
  final String userName;

  final VoidCallback? onViewAllAppointments;

  const HomeTab({
    super.key,
    required this.userName,
    this.onViewAllAppointments,
  });

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      padding: EdgeInsets.fromLTRB(20.w, 12.h, 20.w, 24.h),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const HomeHeader(hasUnreadNotifications: true),
          SizedBox(height: 20.h),
          HomeGreeting(userName: userName),
          SizedBox(height: 20.h),
          const AiNavigatorCard(),
          SizedBox(height: 24.h),
          BlocBuilder<AppointmentsCubit, AppointmentsState>(
            builder: (context, state) {
              return UpcomingCareSection(
                appointments: state.byFilter(AppointmentFilter.upcoming),
                onViewAll: onViewAllAppointments,
              );
            },
          ),
          SizedBox(height: 24.h),
          const CareServicesGrid(services: HomeMockData.services),
          SizedBox(height: 24.h),
          BlocProvider(
            create: (context) => HomeProvidersCubit(
              providerRepository: context.read<ProviderRepository>(),
            )..load(),
            child: BlocBuilder<HomeProvidersCubit, HomeProvidersState>(
              builder: (context, state) {
                return TopProvidersSection(
                  providers: state.providers,
                  isLoading: state.status == HomeProvidersStatus.loading,
                  hasError: state.status == HomeProvidersStatus.failure,
                  onRetry: () => context.read<HomeProvidersCubit>().load(),
                  onBookSlot: (provider) async {
                    final appointments = context.read<AppointmentsCubit>();
                    await context.push('/provider', extra: provider);
                    // The patient may have booked a visit on that screen.
                    if (!appointments.isClosed) await appointments.load();
                  },
                );
              },
            ),
          ),
        ],
      ),
    );
  }
}
