import 'dart:convert';
import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;

import 'request.dart';

void main() {
  runApp(const MyApp());
}

const String apiUrl = 'https://localhost:7028/'; // backend-api

class MyApp extends StatelessWidget {
  
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Flutter ChatGPT App',
      theme: ThemeData(
        primarySwatch: Colors.blue,
      ),
      home: const LoginPage(),
    );
  }
}

class LoginPage extends StatefulWidget {
  
  const LoginPage({super.key});

  @override
  _LoginPageState createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  
  final TextEditingController _usernameController = TextEditingController();
  final TextEditingController _passwordController = TextEditingController();

  Future<void> _login() async {
    final String username = _usernameController.text;
    final String password = _passwordController.text;

    final response = await loginUser(username, password) ;

    if (response != null) {
      // User authenticated successfully
      Navigator.push( // pushReplacement( this replaces the current screen with the new one, and doesn't keep it in the stack
        context,
        MaterialPageRoute(builder: (context) => HomePage()),
      );
    } else {
      // Authentication failed
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Authentication failed'),
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Login'),
      ),
      body: LayoutBuilder(
        builder: (BuildContext context, BoxConstraints constraints) {
          // Check the screen width to determine the layout
          if (constraints.maxWidth < 600) {
            // Mobile layout
            return _buildMobileLayout();
          } else {
            // Desktop layout
            return _buildDesktopLayout();
          }
        },
      ),
    );
  }

  Widget _buildMobileLayout() {
    return Padding(
      padding: const EdgeInsets.all(20.0),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          TextField(
            controller: _usernameController,
            decoration: const InputDecoration(
              labelText: 'Username',
            ),
          ),
          const SizedBox(height: 20),
          TextField(
            controller: _passwordController,
            obscureText: true,
            decoration: const InputDecoration(
              labelText: 'Password',
            ),
          ),
          const SizedBox(height: 20),
          ElevatedButton(
            onPressed: _login,
            child: const Text('Login'),
          ),
        ],
      ),
    );
  }

  Widget _buildDesktopLayout() {
    return Center(
      child: SizedBox(
        width: 400,
        child: Card(
          elevation: 8,
          child: Padding(
            padding: const EdgeInsets.all(20.0),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextField(
                  controller: _usernameController,
                  decoration: const InputDecoration(
                    labelText: 'Username',
                  ),
                ),
                const SizedBox(height: 20),
                TextField(
                  controller: _passwordController,
                  obscureText: true,
                  decoration: const InputDecoration(
                    labelText: 'Password',
                  ),
                ),
                const SizedBox(height: 20),
                ElevatedButton(
                  onPressed: _login,
                  child: const Text('Login'),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

}

class HomePage extends StatefulWidget {
  
  const HomePage({super.key});

  @override
  _HomePageState createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  
  final TextEditingController _queryController = TextEditingController();

  bool _isSendingQuery = false;
  bool _isGettingQueries = false;

  Future<void> _sendQuery(BuildContext context, String query) async {
    setState(() {
      _isSendingQuery = true;
    });

    if (query.length > 0) {
      _queryController.clear();

      final response = await postQuery(query);

      if (response != null) {
        // Display response to user
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: SingleChildScrollView(
              scrollDirection: Axis.vertical, // Scroll vertically
              child: Container(
                padding: const EdgeInsets.all(8.0), // Add padding for better readability
                child: Text(
                  response['data'],
                  textAlign: TextAlign.left,
                ),
              ),
            ), // Text(response['data']),
            duration: response['data'] == 'ChatGPT Error.' 
              ? const Duration(seconds: 3) 
              : const Duration(seconds: 10),
          ),
        );
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Error occurred'),
          ),
        );
      }
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Input a Query'),
        ),
      );
    }

    setState(() {
      _isSendingQuery = false;
    });
  }

  Future<void> _getQueries(BuildContext context) async {

    setState(() {
      _isGettingQueries = true;
    });

    final response = await getUserQueries();

    if (response != null && response['data']?.length > 0) {

      // Create snack bars for each item in the response
      List<SnackBar> snackBars = [];
      
      response['data'].forEach((query) {
        snackBars.add(
          SnackBar(
            content: Text('${query['queryText']} : ${query['responseText']}'),
            duration: const Duration(seconds: 10),
          ),
        );
      });

      // Show all snack bars simultaneously
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Column(
            mainAxisSize: MainAxisSize.max,
            children: snackBars.map((snackBar) => snackBar.content!).toList(),
          ),
          duration: const Duration(seconds: 10),
        ),
      );

    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Error occurred'),
        ),
      );
    }

    setState(() {
      _isGettingQueries = false;
    });

  }

  Future<void> _logout() async {
    final response = await logout();
    // work with response, if required
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('ChatGPT App'),
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            onPressed: () {
              _logout();
              Navigator.pop(context);
            },
          ),
        ],
      ),
      body: Padding(
        padding: const EdgeInsets.all(20.0),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            TextField(
              controller: _queryController,
              decoration: const InputDecoration(
                labelText: 'Enter your query',
              ),
            ),
            const SizedBox(height: 20),
            _isSendingQuery
                ? const SizedBox(
                    height: 24,
                    width: 24,
                    child: CircularProgressIndicator(
                      strokeWidth: 3
                    )
                  )
                : ElevatedButton(
                    onPressed: () => _sendQuery(context, _queryController.text),
                    child: const Text('Send Query'),
                  ),
            const SizedBox(height: 20),
            _isGettingQueries
                ? const SizedBox(
                    height: 24,
                    width: 24,
                    child: CircularProgressIndicator(
                      strokeWidth: 3
                    )
                  )
                : ElevatedButton(
                    onPressed: () => _getQueries(context),
                    child: const Text('Show Past Queries'),
                  )
          ],
        ),
      ),
    );
  }
}
