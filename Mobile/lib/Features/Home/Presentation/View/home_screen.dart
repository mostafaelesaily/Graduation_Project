import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:flutter_screenutil/flutter_screenutil.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Features/Appointments/Presentation/View/appointments_screen.dart';
import 'package:medlink/Features/Home/Presentation/Cubit/home_cubit.dart';
import 'package:medlink/Features/Home/Presentation/Cubit/home_state.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/home_bottom_nav.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/home_header.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/home_tab.dart';
import 'package:medlink/Features/Search/Presentation/View/search_tab.dart';

class HomeScreen extends StatefulWidget {
  /// TODO: replace with the logged-in user's name once the profile endpoint
  /// is wired in.
  final String userName;

  const HomeScreen({super.key, this.userName = 'Sarah'});

  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  int _currentIndex = 0;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: ColorsManager.primaryBackgroundColor,
      body: SafeArea(
        bottom: false,
        child: Column(
          children: [
            Padding(
              padding: const EdgeInsets.only(top: 8.0, left: 8, right: 8),
              child: const HomeHeader(hasUnreadNotifications: true),
            ),
            SizedBox(height: 20.h),
            Expanded(
              child: IndexedStack(
                index: _currentIndex,
                children: [
                  BlocConsumer<HomeCubit, HomeState>(
                    listener: (context, state) {
                      if (state.status == HomeStatus.aiSearch) {
                        setState(() {
                          _currentIndex = 1;
                        });
                      }
                    },
                    builder: (context, state) {
                      if (state.status == HomeStatus.home) {
                        return HomeTab(userName: widget.userName);
                      } else if (state.status == HomeStatus.search) {
                        return SearchTab(searchQuery: state.searchQuery);
                      }
                      return SizedBox();
                    },
                  ),
                  const _ComingSoonTab(title: 'Search'),
                  const AppointmentsScreen(),
                  const _ComingSoonTab(title: 'Records'),
                  const _ComingSoonTab(title: 'Profile'),
                ],
              ),
            ),
          ],
        ),
      ),
      bottomNavigationBar: HomeBottomNav(
        currentIndex: _currentIndex,
        appointmentsBadge: 2,
        onTap: (index) {
          setState(() => _currentIndex = index);
          if (index == 0) {
            context.read<HomeCubit>().changeHomeStatus(status: HomeStatus.home);
          }
        },
      ),
    );
  }
}

class _ComingSoonTab extends StatelessWidget {
  final String title;

  const _ComingSoonTab({required this.title});

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Text(
        '$title - coming soon',
        style: const TextStyle(color: ColorsManager.fadedText, fontSize: 16),
      ),
    );
  }
}
