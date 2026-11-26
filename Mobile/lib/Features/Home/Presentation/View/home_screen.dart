import 'package:flutter/material.dart';
import 'package:medlink/Core/Style/colors_manager.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/home_bottom_nav.dart';
import 'package:medlink/Features/Home/Presentation/View/widgets/home_tab.dart';

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
        child: IndexedStack(
          index: _currentIndex,
          children: [
            HomeTab(userName: widget.userName),
            const _ComingSoonTab(title: 'Search'),
            const _ComingSoonTab(title: 'Appointments'),
            const _ComingSoonTab(title: 'Records'),
            const _ComingSoonTab(title: 'Profile'),
          ],
        ),
      ),
      bottomNavigationBar: HomeBottomNav(
        currentIndex: _currentIndex,
        appointmentsBadge: 2,
        onTap: (index) => setState(() => _currentIndex = index),
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
